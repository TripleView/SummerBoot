
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
using SummerBoot.Feign.Attributes;

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

            services.TryAddSingleton<IRepositoryProxyBuilder, RepositoryProxyBuilder>();

            var types = Assembly.GetCallingAssembly().GetExportedTypes()
                .Union(Assembly.GetExecutingAssembly().GetExportedTypes()).Distinct().ToList();

            var autoRepositoryTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<AutoRepositoryAttribute>() != null).ToList();

            foreach (var type in autoRepositoryTypes)
            {
                services.AddSummerBootRepositoryService(type, ServiceLifetime.Scoped);
            }

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

            services.TryAddSingleton<IClient, IClient.DefaultFeignClient>();
            services.TryAddSingleton<IFeignEncoder, IFeignEncoder.DefaultEncoder>();
            services.TryAddSingleton<IFeignDecoder, IFeignDecoder.DefaultDecoder>();
            services.TryAddSingleton<IFeignProxyBuilder, FeignProxyBuilder>();
            services.AddScoped<HttpService>();
            HttpHeaderSupport.Init();

            var types = Assembly.GetCallingAssembly().GetExportedTypes()
                .Union(Assembly.GetExecutingAssembly().GetExportedTypes()).Distinct().ToList();

            var feignTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<FeignClientAttribute>() != null).ToList();

            foreach (var type in feignTypes)
            {
                services.AddSummerBootFeignService(type, ServiceLifetime.Scoped);
            }
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

            var fallBack = feignClient.FallBack;
            if (fallBack != null)
            {
                if (!fallBack.GetInterfaces().Contains(serviceType)) throw new Exception("fallback must implement " + serviceType.Name);
                services.AddScoped(serviceType, fallBack);
            }

            feignClient.Name = feignClient.Name.GetValueOrDefault(serviceType.FullName);

            var httpClient = services.AddHttpClient(feignClient.Name, it =>
            {
                if (feignClient.Timeout.HasValue)
                {
                    it.Timeout = TimeSpan.FromSeconds(feignClient.Timeout.Value);
                }
            });
            //忽略https证书
            if (feignClient.IsIgnoreHttpsCertificateValidate)
            {
                httpClient.ConfigurePrimaryHttpMessageHandler(it => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            }


            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }
    }
}