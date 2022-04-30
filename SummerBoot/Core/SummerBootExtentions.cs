
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SummerBoot.Core.MvcExtension;
using SummerBoot.Feign;
using SummerBoot.Repository;
using SummerBoot.Resource;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using SummerBoot.Feign.Attributes;
using SummerBoot.Repository.Generator;
using SummerBoot.Repository.Generator.Dialect;
using SummerBoot.Repository.Generator.Dialect.Oracle;
using SummerBoot.Repository.Generator.Dialect.SqlServer;

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
            services.TryAddScoped<IDbFactory, DbFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton(t => option);
            RepositoryOption.Instance = option;

            services.AddScoped(typeof(BaseRepository<>));
            //services.AddSbSingleton<IDataSource, DruidDataSource>();
            services.AddScoped<RepositoryService>();
            services.AddScoped<IDbGenerator, DbGenerator>();
            if (option.IsSqlServer)
            {
                services.AddScoped<IDatabaseFieldMapping, SqlServerDatabaseFieldMapping>();
                services.AddScoped<IDatabaseInfo, SqlServerDatabaseInfo>();
               
            }
            else if (option.IsOracle)
            {
                services.AddScoped<IDatabaseFieldMapping, OracleDatabaseFieldMapping>();
                services.AddScoped<IDatabaseInfo, OracleDatabaseInfo>();
            }


            var repositoryProxyBuilder = new RepositoryProxyBuilder();

            var types = Assembly.GetCallingAssembly().GetExportedTypes()
                .Union(Assembly.GetExecutingAssembly().GetExportedTypes()).Distinct().ToList();

            var autoRepositoryTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<AutoRepositoryAttribute>() != null).ToList();

            foreach (var type in autoRepositoryTypes)
            {
                repositoryProxyBuilder.InitInterface(type);
                services.AddSummerBootRepositoryService(type, ServiceLifetime.Scoped);
            }

            services.TryAddSingleton<IRepositoryProxyBuilder>(it => repositoryProxyBuilder);

            return services;
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

        public static IServiceCollection AddSummerBootFeign(this IServiceCollection services)
        {
            services.AddHttpClient();

            services.TryAddTransient<IClient, IClient.DefaultFeignClient>();
            services.TryAddSingleton<IFeignEncoder, IFeignEncoder.DefaultEncoder>();
            services.TryAddSingleton<IFeignDecoder, IFeignDecoder.DefaultDecoder>();
            
            services.TryAddTransient<HttpService>();
            HttpHeaderSupport.Init();

            var feignProxyBuilder = new FeignProxyBuilder();
            var types = Assembly.GetCallingAssembly().GetExportedTypes()
                .Union(Assembly.GetExecutingAssembly().GetExportedTypes()).Distinct().ToList();

            var feignTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<FeignClientAttribute>() != null).ToList();

            foreach (var type in feignTypes)
            {
                feignProxyBuilder.InitInterface(type);
                services.AddSummerBootFeignService(type, ServiceLifetime.Transient);
            }
            services.TryAddSingleton<IFeignProxyBuilder>(it=> feignProxyBuilder);
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
                 if (!typeof(Task<>).IsAssignableFrom(methodInfo.ReturnType)&& !typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
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

            feignClient.Name = feignClient.Name.GetValueOrDefault(serviceType.FullName);


            if (feignClient.InterceptorType != null)
            {
                if (!feignClient.InterceptorType.GetInterfaces().Any(it => typeof(IRequestInterceptor) == it))
                {
                    throw new Exception($"{serviceType.FullName} must inherit from interface IRequestInterceptor"  );
                }

                services.AddTransient(feignClient.InterceptorType);
            }

            var httpClient = services.AddHttpClient(feignClient.Name, it =>
            {
                if (feignClient.Timeout!=0)
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