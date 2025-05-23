﻿using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.ExpressionParser.Parser;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class RepositoryProxyBuilder : IRepositoryProxyBuilder
    {
        public static Dictionary<string, MethodInfo> MethodsCache { get; set; } = new Dictionary<string, MethodInfo>();
        private static ConcurrentDictionary<string, object> LockObjCache { get; set; } = new ConcurrentDictionary<string, object>();
        private Type targetType;
        private readonly object lockObj = new object();

        private static ConcurrentDictionary<string, Type> TargetTypeCache { set; get; } =
            new ConcurrentDictionary<string, Type>();

        //IRepository接口里的固定方法名
        private string[] solidMethodNames = new string[] { "FastBatchInsertAsync","FastBatchInsert", "ToListAsync","ToPage","ToPageAsync", "InternalQueryListAsync", "InternalQueryPage", "InternalQueryPageAsync", "InternalExecute", "InternalExecuteAsync", "InternalQuery", "InternalQueryAsync", "InternalQueryList", "ExecuteUpdateAsync","ExecuteUpdate", "set_SelectItems","get_SelectItems", "get_Provider", "get_ElementType", "get_Expression", "GetEnumerator", "GetAll", "Get", "Insert", "BatchInsert", "Update", "BatchUpdate", "Delete", "BatchDelete", "GetAllAsync", "GetAsync", "InsertAsync", "BatchInsertAsync",
            "UpdateAsync", "BatchUpdateAsync", "DeleteAsync", "BatchDeleteAsync", "FirstOrDefaultAsync",
            nameof(IRepository<BaseEntity>.FirstAsync),
            nameof(IRepository<BaseEntity>.MaxAsync),
            nameof(IRepository<BaseEntity>.MinAsync),
                nameof(IRepository<BaseEntity>.SumAsync),
                nameof(IRepository<BaseEntity>.AverageAsync),
                nameof(IRepository<BaseEntity>.CountAsync),
               "set_MultiQueryContext",
               "get_MultiQueryContext",
        };

        public object Build(Type interfaceType, params object[] constructor)
        {
            throw new NotImplementedException();
            var cacheKey = interfaceType.FullName;
            Type resultType;

            TargetTypeCache.TryGetValue(cacheKey, out resultType);
            //var resultType= TargetTypeCache.GetOrAdd(cacheKey, it => BuildTargetType(interfaceType, constructor));
            var result = Activator.CreateInstance(resultType, args: constructor);
            return result;
        }

        public object Build(Type interfaceType, Type customBaseRepositoryType, Type repositoryServiceType, params object[] constructor)
        {
            var cacheKey = GetCacheKey(interfaceType, customBaseRepositoryType, repositoryServiceType);
            TargetTypeCache.TryGetValue(cacheKey, out var resultType);
            //var resultType= TargetTypeCache.GetOrAdd(cacheKey, it => BuildTargetType(interfaceType, constructor));
            var result = Activator.CreateInstance(resultType, args: constructor);
            return result;
        }

        public void InitInterface(Type interfaceType, Type customBaseRepositoryType, Type repositoryServiceType)
        {
            var cacheKey = GetCacheKey(interfaceType, customBaseRepositoryType, repositoryServiceType);
            var resultType = BuildTargetType(interfaceType, customBaseRepositoryType, repositoryServiceType);
            TargetTypeCache.TryAdd(cacheKey, resultType);
        }

        public string GetCacheKey(Type interfaceType, Type customBaseRepositoryType, Type repositoryServiceType)
        {
            var cacheKey = interfaceType.FullName + ":" + customBaseRepositoryType.FullName + ":" + repositoryServiceType.FullName;
            return cacheKey;
        }
        /// <summary>
        /// 动态生成接口的实现类
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        private Type BuildTargetType(Type interfaceType, Type customBaseRepositoryType, Type repositoryServiceType)
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

            var allInterfaces = targetType.GetInterfaces();

            List<MethodInfo> targetMethods = new List<MethodInfo>() { };
            targetMethods.AddRange(targetType.GetMethods());

            var isRepository = false;
            Type baseRepositoryType = null;
            foreach (var iInterface in allInterfaces)
            {
                if (iInterface.IsGenericType)
                {
                    isRepository = typeof(IBaseRepository<>).IsAssignableFrom(iInterface.GetGenericTypeDefinition());

                    if (isRepository)
                    {
                        targetMethods.AddRange(iInterface.GetMethods());
                        var genericType = iInterface.GetGenericArguments().First();
                        baseRepositoryType = customBaseRepositoryType.MakeGenericType(genericType);
                        targetMethods.AddRange(typeof(IEnumerable<>).MakeGenericType(genericType).GetMethods());
                        targetMethods.AddRange(typeof(IEnumerable).GetMethods());
                        targetMethods.AddRange(typeof(IQueryable).GetMethods());
                        var c = typeof(IRepository<>).MakeGenericType(genericType).GetMethods();
                        targetMethods.AddRange(typeof(IRepository<>).MakeGenericType(genericType).GetMethods());
                        targetMethods.AddRange(typeof(IDbExecuteAndQuery).GetMethods());
                        targetMethods.AddRange(typeof(IAsyncQueryable<>).MakeGenericType(genericType).GetMethods());
                        break;
                    }
                }

            }

            FieldBuilder baseRepositoryField = null;
            if (isRepository)
            {
                //定义一个字段存放baseRepository
                baseRepositoryField = typeBuilder.DefineField("baseRepository",
                baseRepositoryType, FieldAttributes.Public);
            }

            //定义一个字段存放repositoryService
            var repositoryType = repositoryServiceType;
            FieldBuilder repositoryServiceField = typeBuilder.DefineField("repositoryService",
                repositoryType, FieldAttributes.Public);

            //定义一个字段存放IServiceProvider
            var iServiceProviderType = typeof(IServiceProvider);
            FieldBuilder serviceProviderField = typeBuilder.DefineField("iServiceProvider",
                iServiceProviderType, FieldAttributes.Public);

            //定义一个集合存放参数集合
            FieldBuilder paramterArrField = typeBuilder.DefineField("paramterArr",
                typeof(List<object>), FieldAttributes.Public);

            var ctorParameterTypes = isRepository ? new Type[] { repositoryType, iServiceProviderType, baseRepositoryType } : new Type[] { repositoryType, iServiceProviderType };
            //创建构造函数
            ConstructorBuilder constructorBuilder =
                typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorParameterTypes);

            //il创建构造函数，对httpService和IServiceProvider两个字段进行赋值，同时初始化存放参数的集合
            ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Ldarg_1);
            ilgCtor.Emit(OpCodes.Stfld, repositoryServiceField);

            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Ldarg_2);
            ilgCtor.Emit(OpCodes.Stfld, serviceProviderField);

            if (isRepository)
            {
                ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
                ilgCtor.Emit(OpCodes.Ldarg_3);
                ilgCtor.Emit(OpCodes.Stfld, baseRepositoryField);
            }

            ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            ilgCtor.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructors().First());
            ilgCtor.Emit(OpCodes.Stfld, paramterArrField);
            ilgCtor.Emit(OpCodes.Ret); //返回

            var solidMethods = typeof(IBaseRepository<>).GetMethods();

            foreach (MethodInfo targetMethod in targetMethods)
            {
                //只挑出virtual的方法
                if (targetMethod.IsVirtual)
                {
                    var methodName = targetMethod.Name;
                    //缓存接口的方法体，便于后续将方法体传递给httpService
                    string methodKey = Guid.NewGuid().ToString();
                    MethodsCache[methodKey] = targetMethod;

                    //得到方法的各个参数的类型和参数
                    var paramInfo = targetMethod.GetParameters();
                    var parameterType = paramInfo.Select(it => it.ParameterType).ToArray();
                    var returnType = targetMethod.ReturnType;

                    var underType = returnType.IsGenericType ? returnType.GetGenericArguments().First() : returnType;
                    var isInterface = underType.IsInterface;
                    if (isInterface && returnType.IsGenericType) throw new Exception("return type no support interface");
                    //通过emit生成方法体

                    var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;

                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, methodAttributes, targetMethod.ReturnType, parameterType);
                    //如果是泛型方法，还需要添加泛型参数
                    if (targetMethod.IsGenericMethod)
                    {
                        var genericArgumentNames = targetMethod.GetGenericArguments().Select(it => it.Name).ToArray();
                        methodBuilder.DefineGenericParameters(genericArgumentNames);
                    }

                    ILGenerator ilGen = methodBuilder.GetILGenerator();

                    if (isRepository && solidMethodNames.Any(it => it == methodName))
                    {
                        //先加载类，再加载字段，再加载方法参数，最后调用方法
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldfld, baseRepositoryField);
                        for (int i = 0; i < parameterType.Length; i++)
                        {
                            ilGen.Emit(OpCodes.Ldarg, i + 1);
                        }

                        var selectSolidMethods = baseRepositoryType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(it =>
                            CompareTwoMethod(it, targetMethod)).ToList();

                        //it.ReturnType?.Name == targetMethod.ReturnType?.Name &&

                        if (selectSolidMethods.Count > 1)
                        {
                            throw new Exception("find two methods:" + targetMethod.Name);
                        }

                        var selectMethod = selectSolidMethods.First();

                        ilGen.Emit(OpCodes.Call, selectMethod);
                    }
                    else
                    {
                        var isTask = typeof(Task) == returnType;
                        var isVoid = typeof(void) == returnType;
                        var isAsync = typeof(Task).IsAssignableFrom(returnType) && !isTask;
                        var isInt = typeof(int) == returnType;
                        var isTaskInt = typeof(Task<int>) == returnType;
                        var hasReturnValue = !isTask && !isVoid;
                        var interfaceMethodName = targetMethod.Name;
                        var excuteMethodName = "";
                        MethodInfo executeMethod = null;

                        var selectAttribute = targetMethod.GetCustomAttribute<SelectAttribute>();
                        if (selectAttribute != null && !hasReturnValue) throw new Exception(interfaceMethodName + ":The method marked by selectAttribute must have a return value");
                        var deleteAttribute = targetMethod.GetCustomAttribute<DeleteAttribute>();
                        var updateAttribute = targetMethod.GetCustomAttribute<UpdateAttribute>();
                        if ((deleteAttribute != null || updateAttribute != null) && (!isInt && !isVoid && !isTask && !isTaskInt)) throw new Exception(interfaceMethodName + ":The method marked by updateAttribute or deleteAttribute must return int or task<int> or void or task");
                        //只能有一个注解
                        var attributeNumber = 0;
                        if (selectAttribute != null) attributeNumber++;
                        if (deleteAttribute != null) attributeNumber++;
                        if (updateAttribute != null) attributeNumber++;
                        if (attributeNumber == 0) throw new Exception(interfaceMethodName + ":need selectAttribute or updateAttribute or deleteAttribute");
                        if (attributeNumber > 1) throw new Exception(interfaceMethodName + ":selectAttribute or updateAttribute or deleteAttribute There can only be one");
                        //返回类型不同，则处理方法也不同
                        //返回类型不同，则处理方法也不同
                        //如果没有返回值，即为task或者void类型
                        if (!hasReturnValue)
                        {
                            excuteMethodName = isTask ? "ExecuteNoReturnAsync" : "ExecuteNoReturn";
                            var methodTmp = repositoryType.GetMethod(excuteMethodName);
                            if (methodTmp == null) throw new Exception("找不到执行方法");
                            executeMethod = methodTmp;
                        }
                        //其他类型
                        else
                        {
                            if (selectAttribute != null)
                            {
                                Type genericType;
                                genericType = isAsync ? returnType.GenericTypeArguments.First() : returnType;
                                var baseGenericType = returnType.GetUnderlyingType();

                                var isPage = false;
                                if (genericType.IsGenericType)
                                {
                                    var definitionType = genericType.GetGenericTypeDefinition();
                                    isPage = typeof(IPage<>).IsAssignableFrom(definitionType) || typeof(Page<>).IsAssignableFrom(definitionType);
                                }

                                excuteMethodName = isAsync ?
                                    (isPage ? "PageExecuteAsync" : "ExecuteAsync")
                                    : (isPage ? "PageExecute" : "Execute");

                                var methodTmp = repositoryType.GetMethod(excuteMethodName);

                                if (methodTmp == null) throw new Exception("找不到执行方法");

                                executeMethod = isPage ? methodTmp.MakeGenericMethod(baseGenericType) : methodTmp.MakeGenericMethod(genericType, baseGenericType);
                            }
                            else
                            {
                                excuteMethodName = isTaskInt ? "ExecuteReturnCountAsync" : "ExecuteReturnCount";
                                var methodTmp = repositoryType.GetMethod(excuteMethodName);
                                if (methodTmp == null) throw new Exception("找不到执行方法");
                                executeMethod = methodTmp;
                            }
                        }

                        // 栈底放这玩意，加载字段前要加载类实例，即Ldarg_0
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldfld, repositoryServiceField);

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
                            typeof(RepositoryProxyBuilder).GetMethod("get_MethodsCache", BindingFlags.Static | BindingFlags.Public));
                        ilGen.Emit(OpCodes.Ldstr, methodKey);
                        ilGen.Emit(OpCodes.Call, typeof(Dictionary<string, MethodInfo>).GetMethod("get_Item"));

                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldfld, serviceProviderField);

                        ilGen.Emit(OpCodes.Callvirt, executeMethod);
                        //清空list里的参数
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldfld, paramterArrField);
                        ilGen.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Clear"));
                    }

                    // pop the stack if return void
                    //if (targetMethod.ReturnType == typeof(void))
                    //{
                    //    ilGen.Emit(OpCodes.Pop);
                    //}

                    // complete
                    ilGen.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
                }
            }

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 比较2个方法
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private bool CompareTwoMethod(MethodInfo first, MethodInfo second)
        {
            if (first.Name.Contains("MaxAsync") && second.Name.Contains("MaxAsync"))
            {
                var c = 123;
            }
            if (!first.Name.Split(".").Any(x => x == second.Name))
            {
                return false;
            }
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null && second != null || (second == null && first != null))
            {
                return false;
            }
            //判断是否为泛型
            if (first.IsGenericMethod && !second.IsGenericMethod || (!first.IsGenericMethod && second.IsGenericMethod))
            {
                return false;
            }

            if (first.ReturnType?.Name != second.ReturnType?.Name)
            {
                return false;
            }

            if (first.IsGenericMethod && second.IsGenericMethod)
            {
                var firstGenericArgumentsList = first.GetGenericArguments().ToList();
                var secondGenericArgumentsList = second.GetGenericArguments().ToList();
                if (firstGenericArgumentsList.Count != secondGenericArgumentsList.Count)
                {
                    return false;
                }

                for (int i = 0; i < firstGenericArgumentsList.Count; i++)
                {
                    var firstGenericArgument = firstGenericArgumentsList[i];
                    var secondGenericArgument = secondGenericArgumentsList[i];
                    if (firstGenericArgument.Name != secondGenericArgument.Name)
                    {
                        return false;
                    }
                }
            }

            if (!CompareTwoParameterList(first.GetParameters(), second.GetParameters()))
            {
                return false;
            }

            return true;
        }

        private bool CompareTwoParameterList(ParameterInfo[] first, ParameterInfo[] second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null && second != null || (first != null && second == null) || (first.Length != second.Length))
            {
                return false;
            }
            //firstItem.ParameterType.GetGenericArguments()[0].GetGenericArguments()[0] == secondItem.ParameterType.GetGenericArguments()[0].GetGenericArguments()[0]
            for (int i = 0; i < first.Length; i++)
            {
                var firstItem = first[i];
                var secondItem = second[i];
                if ((firstItem.Name != secondItem.Name) || !CompareTwoParameterType(firstItem.ParameterType, secondItem.ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CompareTwoParameterType(Type first, Type second)
        {
            if (first.IsGenericType ^ second.IsGenericType)
            {
                return false;
            }

            if (first.Name != second.Name)
            {
                return false;
            }

            if (first.IsGenericType && second.IsGenericType)
            {
                if (first.GetGenericArguments().Length != second.GetGenericArguments().Length)
                {
                    return false;
                }

                for (int i = 0; i < first.GetGenericArguments().Length; i++)
                {
                    var firstItem = first.GetGenericArguments()[i];
                    var secondItem = first.GetGenericArguments()[i];
                    if (!CompareTwoParameterType(firstItem, secondItem))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return first == second;
            }

            return true;
        }

        public T Build<T>(params object[] constructor)
        {
            return (T)this.Build(typeof(T), constructor);
        }
    }
}