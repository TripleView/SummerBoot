
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SummerBoot.Core.MvcExtension;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;
using SummerBoot.Feign.Nacos;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Repository.Generator.Dialect;
using SummerBoot.Repository.Generator.Dialect.Oracle;
using SummerBoot.Repository.Generator.Dialect.Sqlite;
using SummerBoot.Repository.Generator.Dialect.SqlServer;
using SummerBoot.Repository.TypeHandler;
using SummerBoot.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Cache;
using SummerBoot.Repository.TypeHandler.Dialect.Oracle;
using SummerBoot.Repository.TypeHandler.Dialect.Sqlite;
using SummerBoot.Repository.TypeHandler.Dialect.SqlServer;
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

        public static IServiceCollection AddSummerBootRepository(this IServiceCollection services, Action<RepositoryOption> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var option = new RepositoryOption();

            action(option);

            if (option.ConnectionString.IsNullOrWhiteSpace()) throw new Exception("ConnectionString is Require");

            if (option.DbConnectionType == null) throw new Exception("DbConnectionType is Require");

            ResetDapperSqlMapperTypeMapDictionary();

            services.TryAddScoped<IDbFactory, DbFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton(t => option);
            RepositoryOption.Instance = option;

            services.AddScoped(typeof(BaseRepository<>));
            //services.AddSbSingleton<IDataSource, DruidDataSource>();
            services.AddScoped<RepositoryService>();

            services.AddTransient<IDbGenerator, DbGenerator>();
            if (option.IsSqlServer)
            {
                services.AddTransient<IDatabaseFieldMapping, SqlServerDatabaseFieldMapping>();
                services.AddTransient<IDatabaseInfo, SqlServerDatabaseInfo>();
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
            else if (option.IsOracle)
            {
                services.AddTransient<IDatabaseFieldMapping, OracleDatabaseFieldMapping>();
                services.AddTransient<IDatabaseInfo, OracleDatabaseInfo>();

            }
            else if (option.IsMysql)
            {
                services.AddTransient<IDatabaseFieldMapping, MysqlDatabaseFieldMapping>();
                services.AddTransient<IDatabaseInfo, MysqlDatabaseInfo>();
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
            else if (option.IsSqlite)
            {
                services.AddTransient<IDatabaseFieldMapping, SqliteDatabaseFieldMapping>();
                services.AddTransient<IDatabaseInfo, SqliteDatabaseInfo>();
            }


            var repositoryProxyBuilder = new RepositoryProxyBuilder();

            var autoRepositoryTypes = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => !it.IsDynamic).ToList();
            foreach (var assembly in allAssemblies)
            {
                autoRepositoryTypes.AddRange(assembly.GetExportedTypes().Where(it => it.IsInterface && it.GetCustomAttribute<AutoRepositoryAttribute>() != null).ToList());
            }

            foreach (var type in autoRepositoryTypes)
            {

                var baseRepositoryType = type.GetInterfaces().FirstOrDefault(it =>
                    it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IBaseRepository<>));
                if (baseRepositoryType == null)
                {
                    continue;
                }

                //注册dapper映射
                RegisterDapperTypeMapTAndHandler(baseRepositoryType, option);
                repositoryProxyBuilder.InitInterface(type);
                services.AddSummerBootRepositoryService(type, ServiceLifetime.Scoped);
            }

            services.TryAddSingleton<IRepositoryProxyBuilder>(it => repositoryProxyBuilder);

            return services;
        }
        /// <summary>
        /// 重置dapper里类型映射的数组
        /// </summary>
        private static void ResetDapperSqlMapperTypeMapDictionary()
        {
            var typeMapField = typeof(SqlMapper).GetField("typeMap", BindingFlags.NonPublic | BindingFlags.Static);
            typeMapField.SetValue(null, new Dictionary<Type, DbType?>(37)
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = null,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = null,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            });

            var resetTypeHandlersMethod = typeof(SqlMapper).GetMethod("ResetTypeHandlers", BindingFlags.NonPublic | BindingFlags.Static);
            resetTypeHandlersMethod.Invoke(null, new object?[1] { false });
        }
        /// <summary>
        /// 注册dapper类型映射和类型处理程序
        /// </summary>
        /// <param name="baseRepositoryType"></param>
        private static void RegisterDapperTypeMapTAndHandler(Type baseRepositoryType, RepositoryOption repositoryOption)
        {
            var entityType = baseRepositoryType.GetGenericArguments()[0];

            var map = new CustomPropertyTypeMap(entityType, (type, columnName)
                =>
            {
                return type.GetProperties().FirstOrDefault(prop =>
                    (prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name).ToLower() == columnName.ToLower());
            });

            //oracle
            if (repositoryOption.IsOracle || repositoryOption.IsMysql)
            {
                SqlMapper.RemoveTypeMap(typeof(TimeSpan));
                SqlMapper.AddTypeHandler(typeof(TimeSpan), new TimeSpanTypeHandler());
                if (repositoryOption.IsOracle)
                {
                    SqlMapper.RemoveTypeMap(typeof(bool));
                    SqlMapper.AddTypeHandler(typeof(bool), new BoolNumericTypeHandler());
                    SqlMapper.RemoveTypeMap(typeof(Guid));
                    SqlMapper.AddTypeHandler(typeof(Guid), new OracleGuidTypeHandler());
                }
                else
                {
                    SqlMapper.RemoveTypeMap(typeof(Guid));
                    SqlMapper.AddTypeHandler(typeof(Guid), new GuidTypeHandler());
                }
            }

            if (repositoryOption.IsSqlite)
            {
                SqlMapper.RemoveTypeMap(typeof(Guid));
                SqlMapper.AddTypeHandler(typeof(Guid), new SqliteGuidTypeHandler());
                SqlMapper.RemoveTypeMap(typeof(TimeSpan));
                SqlMapper.AddTypeHandler(typeof(TimeSpan), new SqliteTimeSpanTypeHandler());
            }
            if (repositoryOption.IsSqlServer)
            {
                SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
            }


            Dapper.SqlMapper.SetTypeMap(entityType, map);
        }

        private static IServiceCollection AddSummerBootRepositoryService(this IServiceCollection services, Type serviceType,
            ServiceLifetime lifetime)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            object Factory(IServiceProvider provider)
            {
                var repositoyProxyBuilder = provider.GetService<IRepositoryProxyBuilder>();
                var repositoyService = provider.GetService<RepositoryService>();
                var repositoryType = serviceType.GetInterfaces().FirstOrDefault(it => it.IsGenericType && typeof(IBaseRepository<>).IsAssignableFrom(it.GetGenericTypeDefinition()));
                if (repositoryType != null)
                {
                    var genericType = repositoryType.GetGenericArguments().First();
                    var baseRepositoryType = typeof(BaseRepository<>).MakeGenericType(genericType);
                    var baseRepository = provider.GetService(baseRepositoryType);
                    var proxy1 = repositoyProxyBuilder.Build(serviceType, repositoyService, provider, baseRepository);
                    return proxy1;
                }
                var proxy = repositoyProxyBuilder.Build(serviceType, repositoyService, provider);
                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
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
                services.TryAddScoped<ICache,MemoryCache>();
            }
            else if (cacheOption.IsUseRedis)
            {
                services.AddStackExchangeRedisCache(it =>
                {
                    it.Configuration = cacheOption.RedisConnectionString;
                    it.InstanceName = "sb";
                });
                services.TryAddSingleton<ICache,RedisCache>();
            }

            services.AddSingleton(cacheOption);
   
            
            return services;
        }

        public static IServiceCollection AddSummerBootFeign(this IServiceCollection services, Action<FeignOption> action = null)
        {
            var feignOption = new FeignOption();
            if (action != null)
            {
                action(feignOption);
            }

            services.AddHttpClient();

            services.TryAddTransient<IClient, IClient.DefaultFeignClient>();
            services.TryAddSingleton<IFeignEncoder, IFeignEncoder.DefaultEncoder>();
            services.TryAddSingleton<IFeignDecoder, IFeignDecoder.DefaultDecoder>();

            services.TryAddTransient<HttpService>();

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

                if (registerInstance)
                {
                    services.AddHostedService<NacosBackgroundService>();
                }
            }

            var feignProxyBuilder = new FeignProxyBuilder();
            var feignTypes = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => !it.IsDynamic).ToList(); ;
            foreach (var assembly in allAssemblies)
            {
                feignTypes.AddRange(assembly.GetExportedTypes().Where(it => it.IsInterface && it.GetCustomAttribute<FeignClientAttribute>() != null).ToList());
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
           
            var customHttpClientHandler = new HttpClientHandler()
            {
                UseCookies = false
            };

            //忽略https证书
            if (feignClient.IsIgnoreHttpsCertificateValidate)
            {
                customHttpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            httpClient.ConfigurePrimaryHttpMessageHandler(it => customHttpClientHandler);

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