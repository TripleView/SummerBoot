
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SummerBoot.Cache;
using SummerBoot.Core.MvcExtension;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;
using SummerBoot.Feign.Nacos;
using SummerBoot.Repository;
using SummerBoot.Resource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.Generator;
using SummerBoot.Repository.Generator.Dialect;
using SummerBoot.Repository.Generator.Dialect.Oracle;
using SummerBoot.Repository.Generator.Dialect.Sqlite;
using SummerBoot.Repository.Generator.Dialect.SqlServer;
using Guid = System.Guid;

namespace SummerBoot.Core
{
    public static class SummerBootExtentions
    {
        public static IServiceCollection AddSummerBoot(this IServiceCollection services, CultureInfo cultureInfo = null)
        {
            //设置语言
            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentCulture;
            ResourceManager.cultureInfo = cultureInfo;
            //var f= ResourceManager.InternalGet("err1");
            //Console.WriteLine("进入多语言模式"+f);
            services.AddLogging();

            var types = Assembly.GetCallingAssembly().GetExportedTypes()
                .Union(Assembly.GetExecutingAssembly().GetExportedTypes()).Distinct().ToList();
            var autoRegisterTypes = types.Where(it => it.IsClass && it.GetCustomAttribute<AutoRegisterAttribute>() != null).ToList();

            autoRegisterTypes.ForEach(it =>
            {
                var autoRegisterAttribute = it.GetCustomAttribute<AutoRegisterAttribute>();
                if (autoRegisterAttribute == null) return;
                var interfaceType = autoRegisterAttribute.InterfaceType;
                if (interfaceType == null) throw new ArgumentNullException(it.Name + "对应的接口不能为空");
                if (!it.GetInterfaces().Contains(interfaceType)) throw new Exception(it.Name + "必须继承接口" + interfaceType.Name);

                switch (autoRegisterAttribute.ServiceLifetime)
                {
                    case ServiceLifetime.Scoped:
                        services.AddScoped(interfaceType, it);
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(interfaceType, it);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(interfaceType, it);
                        break;
                }

            });

            return services;
        }

        /// <summary>
        /// 对mvc进行增强操作
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddSummerBootMvcExtension(this IServiceCollection services, Action<SummerBootMvcOption> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var option = new SummerBootMvcOption();

            action(option);

            if (option.UseGlobalExceptionHandle)
            {
                services.Configure<MvcOptions>(it => it.Filters.Add<GlobalExceptionFilter>());
            }

            if (option.UseValidateParameterHandle)
            {
                // 关闭netcore自动处理参数校验机制
                services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
                services.Configure<MvcOptions>(it => it.Filters.Add<ValidateParameterActionFilter>());
            }

            return services;
        }

        private static readonly object lockObj = new object();

        public static IServiceCollection AddSummerBootRepository(this IServiceCollection services, Action<RepositoryOption> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var option = new RepositoryOption();

            action(option);

            if (option.DatabaseUnits.Count == 0)
            {
                throw new ArgumentNullException("please add databaseUnit");
            }
            var repositoryProxyBuilder = new RepositoryProxyBuilder();
            services.TryAddSingleton<IRepositoryProxyBuilder>(it => repositoryProxyBuilder);

            var name = "Repository";
            string assemblyName = name + "ProxyAssembly";
            string moduleName = name + "ProxyModule";
            string typeName = name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);

            //添加数据库单元
            foreach (var optionDatabaseUnit in option.DatabaseUnits)
            {
                var databaseUnit = optionDatabaseUnit.Value;
                //动态生成IDbFactory接口类型
                var iCustomDbFactoryType = GenerateCustomInterface(modBuilder, typeof(IDbFactory));

                //注册工厂
                AddSummerBootRepositoryCustomDbFactory(services, iCustomDbFactoryType, databaseUnit, modBuilder);
                //动态生成ICustomUnitOfWork接口类型
                var customUnitOfWorkType = GenerateCustomUnitOfWork(modBuilder, iCustomDbFactoryType, databaseUnit.IUnitOfWorkType);
                //注册工作单元
                services.AddScoped(databaseUnit.IUnitOfWorkType, customUnitOfWorkType);
                //动态生成仓储基类
                var customBaseRepositoryType = GenerateCustomBaseRepository(modBuilder, databaseUnit.IUnitOfWorkType, iCustomDbFactoryType);
                services.AddScoped(typeof(IBaseRepository<>), customBaseRepositoryType);
                services.AddScoped(customBaseRepositoryType);
                //动态生成RepositoryService
                var repositoryServiceType = GenerateRepositoryService(modBuilder, databaseUnit.IUnitOfWorkType, iCustomDbFactoryType);
                services.AddScoped(repositoryServiceType);
                //添加数据库生成器类
                if (databaseUnit.IDbGeneratorType != null)
                {
                    //动态生成IDatabaseFieldMapping接口类型
                    var iDatabaseFieldMappingType = GenerateCustomInterface(modBuilder, typeof(IDatabaseFieldMapping));
                    var iDatabaseInfoType = GenerateCustomInterface(modBuilder, typeof(IDatabaseInfo));
                    var dbGeneratorType = GenerateCustomDbGenerator(modBuilder, iDatabaseFieldMappingType, iCustomDbFactoryType,
                        iDatabaseInfoType, new Type[] { databaseUnit.IDbGeneratorType });
                    services.AddTransient(databaseUnit.IDbGeneratorType, dbGeneratorType);
                    if (databaseUnit.IsSqlServer)
                    {
                        var customDatabaseFieldMappingType = GenerateClassImplementInterface(modBuilder, typeof(SqlServerDatabaseFieldMapping),
                            iDatabaseFieldMappingType, Type.EmptyTypes);
                        services.AddTransient(iDatabaseFieldMappingType, customDatabaseFieldMappingType);
                        var customDatabaseInfoType = GenerateClassImplementInterface(modBuilder, typeof(SqlServerDatabaseInfo),
                            iDatabaseInfoType, new Type[] { iCustomDbFactoryType });
                        services.AddTransient(iDatabaseInfoType, customDatabaseInfoType);
                        //先缓存SqlBulkCopy的type类型

                        try
                        {
                            var sqlServerAssembly = Assembly.Load("Microsoft.Data.SqlClient");
                            var sqlBulkCopyType = sqlServerAssembly.GetType("Microsoft.Data.SqlClient.SqlBulkCopy");
                            var sqlBulkCopyOptionsType = sqlServerAssembly.GetType("Microsoft.Data.SqlClient.SqlBulkCopyOptions");

                            var constructorInfo = sqlBulkCopyType.GetConstructors().FirstOrDefault(it =>
                                it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType.GetInterfaces()
                                    .Any(x => x == typeof(IDbConnection)));
                            var generateObjectDelegate = SbUtil.BuildGenerateObjectDelegate(constructorInfo);

                            var constructorInfo3 = sqlBulkCopyType.GetConstructors().FirstOrDefault(it =>
                                it.GetParameters().Length == 3 && it.GetParameters()[2].ParameterType.GetInterfaces()
                                    .Any(x => x == typeof(IDbTransaction)));
                            var generateObjectDelegate3 = SbUtil.BuildGenerateObjectDelegate(constructorInfo3);

                            var sqlBulkCopyWriteMethod = sqlBulkCopyType.GetMethods().FirstOrDefault(it =>
                                 it.Name == "WriteToServer" && it.GetParameters().Length == 1 &&
                                 it.GetParameters()[0].ParameterType == typeof(DataTable));
                            var sqlBulkCopyWriteMethodAsync = sqlBulkCopyType.GetMethods().FirstOrDefault(it =>
                                it.Name == "WriteToServerAsync" && it.GetParameters().Length == 1 &&
                                it.GetParameters()[0].ParameterType == typeof(DataTable));

                            var addColumnMappingMethodInfo = sqlBulkCopyType.GetProperty("ColumnMappings").PropertyType.GetMethods()
                                .FirstOrDefault(it => it.Name == "Add" && it.GetParameters().Length == 2 && it.GetParameters()[0].ParameterType == typeof(string)
                                           && it.GetParameters()[1].ParameterType == typeof(string));

                            SbUtil.CacheDictionary.TryAdd("sqlBulkCopyDelegate", generateObjectDelegate);
                            SbUtil.CacheDictionary.TryAdd("sqlBulkCopyDelegate3", generateObjectDelegate3);
                            SbUtil.CacheDictionary.TryAdd("addColumnMappingMethodInfo", addColumnMappingMethodInfo);
                            SbUtil.CacheDictionary.TryAdd("sqlBulkCopyOptionsType", sqlBulkCopyOptionsType);

                            //缓存委托
                            var sqlBulkCopyWriteMethodTypes = new Type[] { sqlBulkCopyType, typeof(DataTable) };
                            var sqlBulkCopyWriteMethodFuncType = Expression.GetActionType(sqlBulkCopyWriteMethodTypes);
                            var sqlBulkCopyWriteMethodDelegate = Delegate.CreateDelegate(sqlBulkCopyWriteMethodFuncType, sqlBulkCopyWriteMethod);
                            SbUtil.CacheDelegateDictionary.TryAdd("sqlBulkCopyWriteMethodDelegate", sqlBulkCopyWriteMethodDelegate);

                            var sqlBulkCopyWriteMethodAsyncTypes = new Type[] { sqlBulkCopyType, typeof(DataTable), typeof(Task) };
                            var sqlBulkCopyWriteMethodAsyncFuncType = Expression.GetFuncType(sqlBulkCopyWriteMethodAsyncTypes);
                            var sqlBulkCopyWriteMethodAsyncDelegate = Delegate.CreateDelegate(sqlBulkCopyWriteMethodAsyncFuncType, sqlBulkCopyWriteMethodAsync);
                            SbUtil.CacheDelegateDictionary.TryAdd("sqlBulkCopyWriteMethodAsyncDelegate", sqlBulkCopyWriteMethodAsyncDelegate);
                        }
                        catch (Exception e)
                        {
                            SbUtil.CacheDictionary.TryAdd("sqlBulkCopyDelegateErr", e);
                        }
                    }
                    else if (databaseUnit.IsOracle)
                    {

                        var customDatabaseFieldMappingType = GenerateClassImplementInterface(modBuilder, typeof(OracleDatabaseFieldMapping),
                            iDatabaseFieldMappingType, Type.EmptyTypes);
                        services.AddTransient(iDatabaseFieldMappingType, customDatabaseFieldMappingType);
                        var customDatabaseInfoType = GenerateClassImplementInterface(modBuilder, typeof(OracleDatabaseInfo),
                            iDatabaseInfoType, new Type[] { iCustomDbFactoryType });
                        services.AddTransient(iDatabaseInfoType, customDatabaseInfoType);
                    }
                    else if (databaseUnit.IsMysql)
                    {
                        var customDatabaseFieldMappingType = GenerateClassImplementInterface(modBuilder, typeof(MysqlDatabaseFieldMapping),
                            iDatabaseFieldMappingType, Type.EmptyTypes);
                        services.AddTransient(iDatabaseFieldMappingType, customDatabaseFieldMappingType);
                        var customDatabaseInfoType = GenerateClassImplementInterface(modBuilder, typeof(MysqlDatabaseInfo),
                            iDatabaseInfoType, new Type[] { iCustomDbFactoryType });
                        services.AddTransient(iDatabaseInfoType, customDatabaseInfoType);

                        try
                        {
                            var mysqlAssembly = Assembly.Load("MySqlConnector");
                            var mysqlBulkCopyType = mysqlAssembly.GetType("MySqlConnector.MySqlBulkCopy");
                            var mySqlBulkCopyResultType = mysqlAssembly.GetType("MySqlConnector.MySqlBulkCopyResult");

                            var mySqlBulkCopyColumnMappingType = mysqlAssembly.GetType("MySqlConnector.MySqlBulkCopyColumnMapping");

                            var mysqlBulkCopyWriteMethod = mysqlBulkCopyType.GetMethods().FirstOrDefault(it =>
                                 it.Name == "WriteToServer" && it.GetParameters().Length == 1 &&
                                 it.GetParameters()[0].ParameterType == typeof(DataTable));
                            var mysqlBulkCopyWriteMethodAsync = mysqlBulkCopyType.GetMethods().FirstOrDefault(it =>
                                it.Name == "WriteToServerAsync" && it.GetParameters().Length == 2 &&
                                it.GetParameters()[0].ParameterType == typeof(DataTable));
                            var addColumnMappingMethodInfo = mysqlBulkCopyType.GetProperty("ColumnMappings").PropertyType.GetMethods()
                                .FirstOrDefault(it => it.Name == "Add" && it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == mySqlBulkCopyColumnMappingType
                                          );

                            //缓存委托
                            var mysqlBulkCopyWriteMethodTypes = new Type[] { mysqlBulkCopyType, typeof(DataTable), typeof(object) };
                            var mysqlBulkCopyWriteMethodFuncType = Expression.GetFuncType(mysqlBulkCopyWriteMethodTypes);
                            var mysqlBulkCopyWriteMethodDelegate = Delegate.CreateDelegate(mysqlBulkCopyWriteMethodFuncType, mysqlBulkCopyWriteMethod);
                            SbUtil.CacheDelegateDictionary.TryAdd("mysqlBulkCopyWriteMethodDelegate", mysqlBulkCopyWriteMethodDelegate);
                            var mySqlBulkCopyResultValueTaskType = typeof(ValueTask<>).MakeGenericType(mySqlBulkCopyResultType);
                            var mysqlBulkCopyWriteMethodAsyncTypes = new Type[] { mysqlBulkCopyType, typeof(DataTable), typeof(CancellationToken), mySqlBulkCopyResultValueTaskType };
                            var mysqlBulkCopyWriteMethodAsyncFuncType = Expression.GetFuncType(mysqlBulkCopyWriteMethodAsyncTypes);
                            var mysqlBulkCopyWriteMethodAsyncDelegate = Delegate.CreateDelegate(mysqlBulkCopyWriteMethodAsyncFuncType, mysqlBulkCopyWriteMethodAsync);
                            SbUtil.CacheDelegateDictionary.TryAdd("mysqlBulkCopyWriteMethodAsyncDelegate", mysqlBulkCopyWriteMethodAsyncDelegate);

                            var addColumnMappingMethodInfoTypes = new Type[] { mysqlBulkCopyType.GetProperty("ColumnMappings").PropertyType, mySqlBulkCopyColumnMappingType };
                            var addColumnMappingMethodInfoType = Expression.GetActionType(addColumnMappingMethodInfoTypes);
                            var addColumnMappingMethodInfoDelegate = Delegate.CreateDelegate(addColumnMappingMethodInfoType, addColumnMappingMethodInfo);
                            SbUtil.CacheDelegateDictionary.TryAdd("addColumnMappingMethodInfoDelegate", addColumnMappingMethodInfoDelegate);

                            SbUtil.CacheDictionary.TryAdd("mysqlBulkCopyWriteMethodAsync", mysqlBulkCopyWriteMethodAsync);
                            SbUtil.CacheDictionary.TryAdd("mySqlBulkCopyColumnMappingType", mySqlBulkCopyColumnMappingType);
                            SbUtil.CacheDictionary.TryAdd("mysqlBulkCopyType", mysqlBulkCopyType);
                            SbUtil.CacheDictionary.TryAdd("mysqlAddColumnMappingMethodInfo", addColumnMappingMethodInfo);

                        }
                        catch (Exception e)
                        {
                            SbUtil.CacheDictionary.TryAdd("mysqlBulkCopyDelegateErr", e);
                        }
                    }
                    else if (databaseUnit.IsSqlite)
                    {
                        var customDatabaseFieldMappingType = GenerateClassImplementInterface(modBuilder, typeof(SqliteDatabaseFieldMapping),
                            iDatabaseFieldMappingType, Type.EmptyTypes);
                        services.AddTransient(iDatabaseFieldMappingType, customDatabaseFieldMappingType);
                        var customDatabaseInfoType = GenerateClassImplementInterface(modBuilder, typeof(SqliteDatabaseInfo),
                            iDatabaseInfoType, new Type[] { iCustomDbFactoryType });
                        services.AddTransient(iDatabaseInfoType, customDatabaseInfoType);
                    }
                    else if (databaseUnit.IsPgsql)
                    {
                        var customDatabaseFieldMappingType = GenerateClassImplementInterface(modBuilder, typeof(PgsqlDatabaseFieldMapping),
                            iDatabaseFieldMappingType, Type.EmptyTypes);
                        services.AddTransient(iDatabaseFieldMappingType, customDatabaseFieldMappingType);
                        var customDatabaseInfoType = GenerateClassImplementInterface(modBuilder, typeof(PgsqlDatabaseInfo),
                            iDatabaseInfoType, new Type[] { iCustomDbFactoryType });
                        services.AddTransient(iDatabaseInfoType, customDatabaseInfoType);
                    }
                }


                var autoRepositoryTypes = databaseUnit.BindRepositoryTypes;
                foreach (var type in autoRepositoryTypes)
                {

                    var baseRepositoryType = type.GetInterfaces().FirstOrDefault(it =>
                        it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IBaseRepository<>));
                    if (baseRepositoryType == null)
                    {
                        continue;
                    }

                    repositoryProxyBuilder.InitInterface(type, customBaseRepositoryType, repositoryServiceType);
                    services.AddSummerBootRepositoryService(type, ServiceLifetime.Scoped, customBaseRepositoryType, repositoryServiceType, databaseUnit);
                }
            }

            services.AddSingleton(t => option);
            RepositoryOption.Instance = option;
            return services;

        }

        private static IServiceCollection AddSummerBootRepositoryService(this IServiceCollection services, Type serviceType,
            ServiceLifetime lifetime, Type customBaseRepositoryType, Type repositoryServiceType, DatabaseUnit databaseUnit)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            object Factory(IServiceProvider provider)
            {
                var repositoyProxyBuilder = (RepositoryProxyBuilder)provider.GetService<IRepositoryProxyBuilder>();
                var repositoyService = (RepositoryService)provider.GetService(repositoryServiceType);
                repositoyService!.SetDatabaseUnit(databaseUnit);
                var repositoryType = serviceType.GetInterfaces().FirstOrDefault(it => it.IsGenericType && typeof(IBaseRepository<>).IsAssignableFrom(it.GetGenericTypeDefinition()));
                if (repositoryType != null)
                {
                    var genericType = repositoryType.GetGenericArguments().First();
                    var baseRepositoryType = customBaseRepositoryType.MakeGenericType(genericType);
                    var baseRepository = provider.GetService(baseRepositoryType);
                    var proxy1 = repositoyProxyBuilder.Build(serviceType, customBaseRepositoryType, repositoryServiceType, repositoyService, provider, baseRepository);
                    return proxy1;
                }
                var proxy = repositoyProxyBuilder.Build(serviceType, customBaseRepositoryType, repositoryServiceType, repositoyService, provider);
                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }


        /// <summary>
        /// 添加自定义数据工厂接口和自定义数据工厂到ioc容器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="databaseUnit"></param>
        /// <param name="modBuilder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static IServiceCollection AddSummerBootRepositoryCustomDbFactory(IServiceCollection services, Type serviceType,
            DatabaseUnit databaseUnit, ModuleBuilder modBuilder)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            var customDbFactoryType = GenerateCustomDbFactory(modBuilder, serviceType);

            object Factory(IServiceProvider provider)
            {
                var customDbFactory = customDbFactoryType.CreateInstance(new object[] { databaseUnit });
                return customDbFactory;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, ServiceLifetime.Scoped);
            services.Add(serviceDescriptor);

            return services;
        }

        private static Type GenerateCustomDbFactory(ModuleBuilder modBuilder, Type interface1)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Class | TypeAttributes.Public;
            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = new Type[] { interface1 };
            parentType = typeof(CustomDbFactory);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("CustomDbFactory" + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { typeof(DatabaseUnit) });
            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);
            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        private static Type GenerateCustomUnitOfWork(ModuleBuilder modBuilder, Type constructorType, Type interfaceType)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = new Type[] { interfaceType };
            parentType = typeof(UnitOfWork);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("CustomUnitOfWork" + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { typeof(ILogger<UnitOfWork>), constructorType });
            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Ldarg_2);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 生成父类为指定类并且实现某接口的新类
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="parentType"></param>
        /// <param name="interfaceType"></param>
        /// <param name="constructorTypes"></param>
        /// <returns></returns>
        private static Type GenerateClassImplementInterface(ModuleBuilder modBuilder, Type parentType, Type interfaceType, Type[] constructorTypes)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //要实现的接口
            Type[] interfaceTypes = new Type[] { interfaceType };


            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType(parentType.Name + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                constructorTypes);
            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            if (constructorTypes.Length >= 1)
            {
                conIl.Emit(OpCodes.Ldarg_1);
            }
            if (constructorTypes.Length >= 2)
            {
                conIl.Emit(OpCodes.Ldarg_2);
            }
            if (constructorTypes.Length >= 3)
            {
                conIl.Emit(OpCodes.Ldarg_3);
            }

            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 生成特殊仓储基类
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="constructorType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        private static Type GenerateCustomBaseRepository(ModuleBuilder modBuilder, Type ICustomUnitOfWorkType, Type ICustomDbFactoryType)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = typeof(CustomBaseRepository<>);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("CustomBaseRepository" + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);
            string[] typeParamNames = { "T" };
            GenericTypeParameterBuilder[] typeParams =
                typeBuilder.DefineGenericParameters(typeParamNames);

            GenericTypeParameterBuilder TFirst = typeParams[0];
            TFirst.SetGenericParameterAttributes(
                GenericParameterAttributes.ReferenceTypeConstraint);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { ICustomUnitOfWorkType, ICustomDbFactoryType });

            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Ldarg_2);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 生成数据库生成器类
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="ICustomUnitOfWorkType"></param>
        /// <param name="ICustomDbFactoryType"></param>
        /// <returns></returns>
        private static Type GenerateCustomDbGenerator(ModuleBuilder modBuilder, Type IDatabaseFieldMappingType, Type ICustomDbFactoryType, Type IDatabaseInfoType, Type[] interfaceTypes)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;

            parentType = typeof(DbGenerator);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("CustomDbGenerator" + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { IDatabaseFieldMappingType, ICustomDbFactoryType, IDatabaseInfoType });

            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Ldarg_2);
            conIl.Emit(OpCodes.Ldarg_3);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 生成RepositoryService类
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="ICustomUnitOfWorkType"></param>
        /// <param name="ICustomDbFactoryType"></param>
        /// <returns></returns>
        private static Type GenerateRepositoryService(ModuleBuilder modBuilder, Type ICustomUnitOfWorkType, Type ICustomDbFactoryType)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = typeof(RepositoryService);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("RepositoryService" + Guid.NewGuid().ToString("N"), newTypeAttribute, parentType, interfaceTypes);
            string[] typeParamNames = { "T" };

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { ICustomUnitOfWorkType, ICustomDbFactoryType });

            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Ldarg_2);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        /// <summary>
        /// 生成继承与特定接口的接口
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        private static Type GenerateCustomInterface(ModuleBuilder modBuilder, Type interfaceType)
        {
            if (interfaceType == null || !interfaceType.IsInterface)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Interface |
                                              TypeAttributes.Abstract |
                                              TypeAttributes.AutoClass |
                                              TypeAttributes.AnsiClass |
                                              TypeAttributes.BeforeFieldInit |
                                              TypeAttributes.AutoLayout;

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType(interfaceType.Name + Guid.NewGuid().ToString("N"), newTypeAttribute, null, new Type[] { interfaceType });
            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        public static IServiceCollection AddSummerBootCache(this IServiceCollection services,
            Action<CacheOption> action = null)
        {
            var cacheOption = new CacheOption();
            if (action != null)
            {
                action(cacheOption);
            }

            if (cacheOption.CacheDeserializer != null)
            {
                services.TryAddScoped(typeof(ICacheDeserializer), cacheOption.CacheDeserializer.GetType());
            }
            else
            {
                services.TryAddScoped<ICacheDeserializer, JsonCacheDeserializer>();
            }


            if (cacheOption.CacheSerializer != null)
            {
                services.TryAddScoped(typeof(ICacheSerializer), cacheOption.CacheSerializer.GetType());
            }
            else
            {
                services.TryAddScoped<ICacheSerializer, JsonCacheSerializer>();
            }

            if (cacheOption.IsUseMemory)
            {
                services.AddMemoryCache();
                services.TryAddScoped<ICache, MemoryCache>();
            }
            else if (cacheOption.IsUseRedis)
            {
                services.AddStackExchangeRedisCache(it =>
                {
                    it.Configuration = cacheOption.RedisConnectionString;
                    it.InstanceName = "sb";
                });
                services.TryAddScoped<ICache, RedisCache>();
            }

            services.AddSingleton(cacheOption);


            return services;
        }

        public static IHostBuilder UseNacosConfiguration(this IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                var config = configurationBuilder.Build();
                configurationBuilder.AddNacosConfiguration(config);
            });

            return builder;
        }

        public static IConfigurationBuilder AddNacosConfiguration(
            this IConfigurationBuilder builder,
            IConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var source = new NacosConfigurationSource(configuration);
            //configuration.Bind(source);

            return builder.Add(source);
        }

        public static IServiceCollection AddSummerBootFeign(this IServiceCollection services, Action<FeignOption> action = null)
        {
            var feignOption = new FeignOption();
            if (action != null)
            {
                action(feignOption);
            }

            services.AddHttpClient();
            services.AddSummerBootCache();
            services.TryAddTransient<IClient, IClient.DefaultFeignClient>();
            services.TryAddSingleton<IFeignEncoder, IFeignEncoder.DefaultEncoder>();
            services.TryAddSingleton<IFeignDecoder, IFeignDecoder.DefaultDecoder>();
            services.TryAddTransient<HttpService>();
            services.TryAddTransient<FeignHttpClientHandler>();
            services.AddSingleton<FeignOption>(it => feignOption);
            HttpHeaderSupport.Init();

            if (feignOption.EnableNacos)
            {
                if (feignOption.Configuration == null)
                {
                    throw new ArgumentNullException("AddNacos must add configuration");
                }

                var nacosConfiguration = feignOption.Configuration.GetSection("nacos");
                services.Configure<NacosOption>(nacosConfiguration);
                var registerInstance = false;
                var registerInstanceString = nacosConfiguration.GetSection("registerInstance").Value;
                bool.TryParse(registerInstanceString, out registerInstance);

                var serviceAddress = nacosConfiguration.GetSection("serviceAddress").Value;
                if (serviceAddress.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException("nacos option serviceAddress can not be null");
                }

                var namespaceId = nacosConfiguration.GetSection("namespaceId").Value;
                if (namespaceId.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException("nacos option namespaceId can not be null");
                }

                if (registerInstance)
                {
                    var groupName = nacosConfiguration.GetSection("groupName").Value;
                    if (groupName.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException("nacos option groupName can not be null");
                    }

                    var serviceName = nacosConfiguration.GetSection("serviceName").Value;
                    if (serviceName.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException("nacos option serviceName can not be null");
                    }
                    services.AddHostedService<NacosBackgroundService>();
                }
            }

            if (feignOption.GlobalInterceptorType != null)
            {
                services.AddTransient(feignOption.GlobalInterceptorType);
            }

            var feignProxyBuilder = new FeignProxyBuilder();
            var feignTypes = new List<Type>();
            var requestInterceptorTypes = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => !it.IsDynamic).ToList();

            foreach (var assembly in allAssemblies)
            {
                requestInterceptorTypes.AddRange(assembly.GetExportedTypes().Where(it => it.IsClass && it.GetInterfaces().FirstOrDefault(x=>x==typeof(IRequestInterceptor))!=null).ToList());
                feignTypes.AddRange(assembly.GetExportedTypes().Where(it => it.IsInterface && it.GetCustomAttribute<FeignClientAttribute>() != null).ToList());
            }

            foreach (var type in requestInterceptorTypes)
            {
                services.TryAddTransient(type);
            }

            foreach (var type in feignTypes)
            {
                feignProxyBuilder.InitInterface(type);
                services.AddSummerBootFeignService(type, ServiceLifetime.Transient);
            }
            services.TryAddSingleton<IFeignProxyBuilder>(it => feignProxyBuilder);
            services.TryAddSingleton<IFeignUnitOfWork, DefaultFeignUnitOfWork>();
            services.AddScoped<IFeignUnitOfWork, DefaultFeignUnitOfWork>();
            return services;
        }

        private static IServiceCollection AddSummerBootFeignService(this IServiceCollection services, Type serviceType,
            ServiceLifetime lifetime)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            object Factory(IServiceProvider provider)
            {
                var feignProxyBuilder = provider.GetService<IFeignProxyBuilder>();
                var httpService = provider.GetService<HttpService>();
                var proxy = feignProxyBuilder.Build(serviceType, httpService, provider);

                return proxy;
            };

            var feignClient = serviceType.GetCustomAttribute<FeignClientAttribute>();
            if (feignClient == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            //判断方法返回类型是不是task<>
            foreach (var methodInfo in serviceType.GetMethods())
            {
                if (!typeof(Task<>).IsAssignableFrom(methodInfo.ReturnType) && !typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw new ArgumentException($"{methodInfo.Name} must return task<>");
                }
            }

            var fallBack = feignClient.FallBack;
            if (fallBack != null)
            {
                if (!fallBack.GetInterfaces().Contains(serviceType)) throw new Exception("fallback must implement " + serviceType.Name);
                services.AddTransient(serviceType, fallBack);
            }

            var name = feignClient.Name.GetValueOrDefault(serviceType.FullName);
            feignClient.SetName(name);

            if (feignClient.InterceptorType != null)
            {
                if (!feignClient.InterceptorType.GetInterfaces().Any(it => typeof(IRequestInterceptor) == it))
                {
                    throw new Exception($"{serviceType.FullName} must inherit from interface IRequestInterceptor");
                }

                services.AddTransient(feignClient.InterceptorType);
            }

            var httpClient = services.AddHttpClient(feignClient.Name, it =>
            {
                if (feignClient.Timeout != 0)
                {
                    it.Timeout = TimeSpan.FromSeconds(feignClient.Timeout);
                }

            });

            httpClient.ConfigurePrimaryHttpMessageHandler(it =>
            {
                var feignHttpClientHandler = it.GetRequiredService<FeignHttpClientHandler>();
                //忽略https证书
                if (feignClient.IsIgnoreHttpsCertificateValidate)
                {
                    feignHttpClientHandler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                return feignHttpClientHandler;
            });

            //httpClient.ConfigurePrimaryHttpMessageHandler<LoggingHandler>();
            //httpClient.ConfigurePrimaryHttpMessageHandler((e) =>
            //{
            //    return new LoggingHandler();
            //});

            //httpClient.Services.Configure<HttpClientFactoryOptions>(httpClient.Name, options =>
            //{
            //    options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler =(HttpMessageHandler)b.Services.GetRequiredService(typeof(string)));
            //});

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }
    }
}