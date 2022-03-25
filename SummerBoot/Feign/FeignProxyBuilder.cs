using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using SummerBoot.Core;

namespace SummerBoot.Feign
{
    public class FeignProxyBuilder : IFeignProxyBuilder
    {
        public static Dictionary<string, MethodInfo> MethodsCache { get; set; } = new Dictionary<string, MethodInfo>();

        private Type targetType;

        private static ConcurrentDictionary<string, Type> TargetTypeCache { set; get; } =
            new ConcurrentDictionary<string, Type>();


        public object Build(Type interfaceType, params object[] constructor)
        {
            var cacheKey = interfaceType.FullName;
            var resultType = TargetTypeCache.GetOrAdd(cacheKey, (s) => BuildTargetType(interfaceType, constructor));
            var result = Activator.CreateInstance(resultType, args: constructor);
            return result;
        }

        /// <summary>
        /// 动态生成接口的实现类
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        private Type BuildTargetType(Type interfaceType, params object[] constructor)
        {
            targetType = interfaceType;
            string assemblyName = targetType.Name + "ProxyAssembly";
            string moduleName = targetType.Name + "ProxyModule";
            string typeName = targetType.Name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);

            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Class | TypeAttributes.Public;
            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes;

            if (targetType.IsInterface)
            {
                parentType = typeof(object);
                interfaceTypes = new Type[] { targetType };
            }
            else
            {
                parentType = targetType;
                interfaceTypes = Type.EmptyTypes;
            }
            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType(typeName, newTypeAttribute, parentType, interfaceTypes);

            //定义一个字段存放httpService
            var httpType = typeof(HttpService);
            FieldBuilder httpServiceField = typeBuilder.DefineField("httpService",
                httpType, FieldAttributes.Public);

            //定义一个字段存放IServiceProvider
            var iServiceProviderType = typeof(IServiceProvider);
            FieldBuilder serviceProviderField = typeBuilder.DefineField("iServiceProvider",
                iServiceProviderType, FieldAttributes.Public);

            //定义一个集合存放参数集合
            FieldBuilder paramterArrField = typeBuilder.DefineField("paramterArr",
                typeof(List<object>), FieldAttributes.Public);
            //创建构造函数
            ConstructorBuilder constructorBuilder =
                typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { httpType, iServiceProviderType });

            //il创建构造函数，对httpService和IServiceProvider两个字段进行赋值，同时初始化存放参数的集合
            ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Ldarg_1);
            ilgCtor.Emit(OpCodes.Stfld, httpServiceField);

            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Ldarg_2);
            ilgCtor.Emit(OpCodes.Stfld, serviceProviderField);

            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructors().First());
            ilgCtor.Emit(OpCodes.Stfld, paramterArrField);
            ilgCtor.Emit(OpCodes.Ret); //返回

            MethodInfo[] targetMethods = targetType.GetMethods();

            foreach (MethodInfo targetMethod in targetMethods)
            {
                //只挑出virtual的方法
                if (targetMethod.IsVirtual)
                {
                    //缓存接口的方法体，便于后续将方法体传递给httpService
                    string methodKey = Guid.NewGuid().ToString();
                    MethodsCache[methodKey] = targetMethod;

                    //得到方法的各个参数的类型和参数
                    var paramInfo = targetMethod.GetParameters();
                    var parameterType = paramInfo.Select(it => it.ParameterType).ToArray();
                    var returnType = targetMethod.ReturnType;
                    //方法返回值只能是task，即只支持异步，因为http操作是io操作
                    if (!typeof(Task).IsAssignableFrom(returnType)) throw new Exception("return type must be task<>");
                    var underType = returnType.IsGenericType ? returnType.GetGenericArguments().First() : returnType;

                    //通过emit生成方法体
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, targetMethod.ReturnType, parameterType);
                    ILGenerator ilGen = methodBuilder.GetILGenerator();

                    MethodInfo executeMethod = null;
                    if (returnType == typeof(Task))
                    {
                        var methodTmp = httpType.GetMethod("ExecuteNoReturnAsync");
                        if (methodTmp == null) throw new Exception("找不到执行方法");
                        executeMethod = methodTmp;
                    }
                    else
                    {
                        var methodTmp = httpType.GetMethod("ExecuteAsync");

                        if (methodTmp == null) throw new Exception("找不到执行方法");
                        executeMethod = methodTmp.IsGenericMethod ? methodTmp.MakeGenericMethod(underType) : methodTmp;
                    }


                    // 栈底放这玩意，加载字段前要加载类实例，即Ldarg_0
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldfld, httpServiceField);

                    //把所有参数都放到list<object>里
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldfld, paramterArrField);
                    for (int i = 0; i < parameterType.Length; i++)
                    {
                        ilGen.Emit(OpCodes.Dup);
                        ilGen.Emit(OpCodes.Ldarg_S, i + 1);
                        if (parameterType[i].IsValueType)
                        {
                            ilGen.Emit(OpCodes.Box, parameterType[i]);
                        }
                        ilGen.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add"));
                    }

                    // 当前栈[httpServiceField paramterArrField]
                    //从缓存里取出方法体
                    ilGen.Emit(OpCodes.Call,
                        typeof(FeignProxyBuilder).GetMethod("get_MethodsCache", BindingFlags.Static | BindingFlags.Public));
                    ilGen.Emit(OpCodes.Ldstr, methodKey);
                    ilGen.Emit(OpCodes.Call, typeof(Dictionary<string, MethodInfo>).GetMethod("get_Item"));

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldfld, serviceProviderField);

                    ilGen.Emit(OpCodes.Callvirt, executeMethod);
                    //清空list里的参数
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldfld, paramterArrField);
                    ilGen.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Clear"));
                    // pop the stack if return void
                    if (targetMethod.ReturnType == typeof(void))
                    {
                        ilGen.Emit(OpCodes.Pop);
                    }

                    // complete
                    ilGen.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
                }
            }

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        public T Build<T>(params object[] constructor)
        {
            return (T)this.Build(typeof(T), constructor);
        }
    }
}