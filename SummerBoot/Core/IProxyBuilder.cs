using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace SummerBoot.Core
{
    public interface IProxyBuilder
    {
        Object Build(Type interfaceType,params object[] constructor);
        T Build<T>(params object[] constructor);
        public class DefaultProxyBuilder : IProxyBuilder
        {
            private Type targetType;
            public object Build(Type interfaceType, params object[] constructor)
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

                //var httpType = typeof(HttpService);
                ////定义一个变量存放httpService
                //FieldBuilder httpServiceField = typeBuilder.DefineField("httpService",
                //    httpType, FieldAttributes.Public);

                ////创建构造函数
                //ConstructorBuilder constructorBuilder =
                //    typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { httpType });

                //ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
                //ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
                //ilgCtor.Emit(OpCodes.Ldarg_1);
                //ilgCtor.Emit(OpCodes.Stfld, httpServiceField);
                //ilgCtor.Emit(OpCodes.Ret); //返回

                //MethodInfo[] targetMethods = targetType.GetMethods();

                //foreach (MethodInfo targetMethod in targetMethods)
                //{
                //    //只挑出virtual的方法
                //    if (targetMethod.IsVirtual)
                //    {
                //        //得到方法的各个参数的类型和参数
                //        ParameterInfo[] paramInfo = targetMethod.GetParameters();
                //        var parameterType = paramInfo.Select(it => it.ParameterType).ToArray();
                //        var returnType = targetMethod.ReturnType;
                //        var underType = returnType.IsGenericType ? returnType.GetGenericArguments().First() : returnType;

                //        //通过emit生成方法体
                //        MethodBuilder methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, targetMethod.ReturnType, parameterType);
                //        ILGenerator ilGen = methodBuilder.GetILGenerator();
                //        ilGen.Emit(OpCodes.Ldarg_0);

                //        var methodTmp = httpType.GetMethod("GetAsync");

                //        if (methodTmp == null) continue;
                //        if (methodTmp.IsGenericMethod)
                //        {
                //            methodTmp.MakeGenericMethod(underType);
                //        }

                //        ilGen.Emit(OpCodes.Call, methodTmp);

                //        // pop the stack if return void
                //        if (targetMethod.ReturnType == typeof(void))
                //        {
                //            ilGen.Emit(OpCodes.Pop);
                //        }

                //        // complete
                //        ilGen.Emit(OpCodes.Ret);
                //        typeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
                //    }
                //}

                var resultType = typeBuilder.CreateTypeInfo().AsType();
                var result = Activator.CreateInstance(resultType, args: constructor);
                return result;
            }

            public T Build<T>(params object[] constructor)
            {
                return (T)this.Build(typeof(T), constructor);
            }
        }
    }

   
}