
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
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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
                RegisterDapperTypeMapTAndHandler(baseRepositoryType,option);
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
            var typeMapField= typeof(SqlMapper).GetField("typeMap",BindingFlags.NonPublic|BindingFlags.Static);
            typeMapField.SetValue(null,new Dictionary<Type, DbType?>(37)
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

            var resetTypeHandlersMethod= typeof(SqlMapper).GetMethod("ResetTypeHandlers", BindingFlags.NonPublic | BindingFlags.Static);
            resetTypeHandlersMethod.Invoke(null, new object?[1]{false});
        }
        /// <summary>
        /// 注册dapper类型映射和类型处理程序
        /// </summary>
        /// <param name="baseRepositoryType"></param>
        private static void RegisterDapperTypeMapTAndHandler(Type baseRepositoryType,RepositoryOption repositoryOption)
        {
            var entityType = baseRepositoryType.GetGenericArguments()[0];
            
            var map = new CustomPropertyTypeMap(entityType, (type, columnName)
                =>
            {
                return type.GetProperties().FirstOrDefault(prop =>
                    (prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name).ToLower() == columnName.ToLower());
            });

            //oracle
            if (repositoryOption.IsOracle||repositoryOption.IsMysql)
            {
                SqlMapper.RemoveTypeMap(typeof(Guid));
                SqlMapper.AddTypeHandler(typeof(Guid), new GuidTypeHandler());
                SqlMapper.RemoveTypeMap(typeof(TimeSpan));
                SqlMapper.AddTypeHandler(typeof(TimeSpan), new TimeSpanTypeHandler());
                if (repositoryOption.IsOracle)
                {
                    SqlMapper.RemoveTypeMap(typeof(bool));
                    SqlMapper.AddTypeHandler(typeof(bool), new BoolNumericTypeHandler());
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
                SqlMapper.RemoveTypeMap(typeof(TimeSpan));
                SqlMapper.AddTypeHandler(typeof(TimeSpan), new SqlServerTimeSpanTypeHandler());
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
                var registerInstanceString= nacosConfiguration.GetSection("registerInstance").Value;
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
            //忽略https证书
            if (feignClient.IsIgnoreHttpsCertificateValidate)
            {
                httpClient.ConfigurePrimaryHttpMessageHandler(it => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
                httpClient.AddHttpMessageHandler(() =>
                {
                    return new LoggingHandler();
                });
            }

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