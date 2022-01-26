using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SqlOnline.Utils;
using StackExchange.Redis;
using SummerBoot.Cache;
using SummerBoot.Feign;
using SummerBoot.Repository;
using SummerBoot.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace SummerBoot.Core
{
    public static class SummerBootExtentions
    {
        /// <summary>
        /// 瞬时
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, interceptorTypes);
        }
        /// <summary>
        /// 瞬时，如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;

            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 瞬时
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 瞬时 如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 请求级别
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求级别 如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求级别
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求级别,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton<TService, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton(this IServiceCollection services, Type serviceType,
            Type implementationType, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Singleton, interceptorTypes);
        }

        public static IServiceCollection AddSbService(this IServiceCollection services, Type serviceType, Type implementationType,
            ServiceLifetime lifetime, params Type[] interceptorTypes)
        {
            services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

            //添加cache拦截器
            var enableCahce = false;
            var methods = implementationType.GetTypeInfo().DeclaredMethods;
            //返回值类型列表
            //var returnModels = new List<Type>();
            foreach (var methodInfo in methods)
            {
                if (methodInfo.GetCustomAttribute<CacheableAttribute>() != null ||
                    methodInfo.GetCustomAttributes<CachePutAttribute>().Any() ||
                    methodInfo.GetCustomAttributes<CacheEvictAttribute>().Any())
                {
                    enableCahce = true;
                }
            }

            object Factory(IServiceProvider provider)
            {
                var target = provider.GetService(implementationType);
                var properties = implementationType.GetTypeInfo().DeclaredProperties;

                foreach (PropertyInfo info in properties)
                {
                    //属性注入
                    var autowiredAttribute = info.GetCustomAttribute<AutowiredAttribute>();
                    if (autowiredAttribute != null)
                    {
                        var isRequire = autowiredAttribute.Require;
                        var propertyType = info.PropertyType;
                        object impl = null;
                        var qualifierAttribute = info.GetCustomAttribute<QualifierAttribute>();
                        if (qualifierAttribute != null)
                        {
                            var serviceName = qualifierAttribute.Name;
                            impl = provider.GetServiceByName(serviceName, propertyType, isRequire);
                        }
                        else
                        {
                            impl = isRequire ? provider.GetRequiredService(propertyType) : provider.GetService(propertyType);
                        }

                        if (impl != null)
                        {
                            info.SetValue(target, impl);
                        }
                    }

                    //配置值注入
                    if (info.GetCustomAttribute<ValueAttribute>() is ValueAttribute valueAttribute)
                    {
                        var value = valueAttribute.Value;
                        if (provider.GetService(typeof(IConfiguration)) is IConfiguration configService)
                        {
                            var pathValue = configService.GetSection(value).Value;
                            if (pathValue != null)
                            {
                                var pathV = Convert.ChangeType(pathValue, info.PropertyType);
                                info.SetValue(target, pathV);
                            }
                        }

                    }
                }


                List<IInterceptor> interceptors = interceptorTypes.ToList()
                    .ConvertAll<IInterceptor>(interceptorType => provider.GetService(interceptorType) as IInterceptor);

                //添加缓存拦截器
                if (enableCahce)
                {
                    var cacheInterceptor = provider.GetService<CacheInterceptor>();
                    interceptors.Add(cacheInterceptor);
                }

                var proxyGenerator = provider.GetService<ProxyGenerator>();
                var proxy = proxyGenerator.CreateInterfaceProxyWithTarget(serviceType, target, interceptors.ToArray());

                if (proxy is IInitializing proxyTmp)
                {
                    proxyTmp.AfterPropertiesSet();
                }

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }

        /// <summary>
        /// 瞬时
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 瞬时,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 瞬时
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient(this IServiceCollection services, Type serviceType, params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 瞬时,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient(this IServiceCollection services, Type serviceType, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, ServiceLifetime.Transient, interceptorTypes);
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求级别,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped(this IServiceCollection services, Type serviceType,
             params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 请求级别,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped(this IServiceCollection services, Type serviceType,
            params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, ServiceLifetime.Scoped, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton<TService>(this IServiceCollection services, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton(this IServiceCollection services, Type serviceType, params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, ServiceLifetime.Singleton, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton(this IServiceCollection services, Type serviceType, params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, ServiceLifetime.Singleton, interceptorTypes);
        }

        public static IServiceCollection AddSbService(this IServiceCollection services, Type serviceType,
            ServiceLifetime lifetime, params Type[] interceptorTypes)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == (Type)null)
                throw new ArgumentNullException(nameof(serviceType));

            //添加cache拦截器
            var enableCahce = false;
            var methods = serviceType.GetTypeInfo().DeclaredMethods;
            //返回值类型列表
            //var requireTypes = new List<Type>();
            foreach (var methodInfo in methods)
            {
                if (methodInfo.GetCustomAttribute<CacheableAttribute>() != null ||
                    methodInfo.GetCustomAttributes<CachePutAttribute>().Any() ||
                    methodInfo.GetCustomAttributes<CacheEvictAttribute>().Any())
                {
                    enableCahce = true;
                }
            }

            object Factory(IServiceProvider provider)
            {


                List<IInterceptor> interceptors = interceptorTypes.ToList()
                    .ConvertAll<IInterceptor>(interceptorType => provider.GetService(interceptorType) as IInterceptor);

                //添加注解拦截器
                if (enableCahce)
                {
                    var cacheInterceptor = provider.GetService<CacheInterceptor>();
                    interceptors.Add(cacheInterceptor);
                }

                var proxyGenerator = provider.GetService<ProxyGenerator>();

                var proxy = proxyGenerator.CreateClassProxy(serviceType, interceptors.ToArray());

                var properties = serviceType.GetTypeInfo().DeclaredProperties;

                foreach (PropertyInfo info in properties)
                {
                    //属性注入
                    var autowiredAttribute = info.GetCustomAttribute<AutowiredAttribute>();
                    if (autowiredAttribute != null)
                    {
                        var isRequire = autowiredAttribute.Require;
                        var propertyType = info.PropertyType;
                        object impl = null;
                        var qualifierAttribute = info.GetCustomAttribute<QualifierAttribute>();
                        if (qualifierAttribute != null)
                        {
                            var serviceName = qualifierAttribute.Name;
                            impl = provider.GetServiceByName(serviceName, propertyType, isRequire);
                        }
                        else
                        {
                            impl = isRequire ? provider.GetRequiredService(propertyType) : provider.GetService(propertyType);
                        }

                        if (impl != null)
                        {
                            info.SetValue(proxy, impl);
                        }
                    }
                    //配置值注入
                    if (info.GetCustomAttribute<ValueAttribute>() is ValueAttribute valueAttribute)
                    {
                        var value = valueAttribute.Value;
                        if (provider.GetService(typeof(IConfiguration)) is IConfiguration configService)
                        {
                            var pathValue = configService.GetSection(value).Value;
                            if (pathValue != null)
                            {
                                var pathV = Convert.ChangeType(pathValue, info.PropertyType);
                                info.SetValue(proxy, pathV);
                            }
                        }
                    }
                }

                if (proxy is IInitializing proxyTmp)
                {
                    proxyTmp.AfterPropertiesSet(); ;
                }

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }

        /// <summary>
        /// 添加summer boot扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddSummerBootMvcExtention(this IMvcBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            ControllerFeature feature = new ControllerFeature();
            builder.PartManager.PopulateFeature<ControllerFeature>(feature);
            foreach (Type type in feature.Controllers.Select<TypeInfo, Type>((Func<TypeInfo, Type>)(c => c.AsType())))
                builder.Services.TryAddTransient(type, type);
            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, SbControllerActivator>());

            return builder;
        }

        public static IServiceCollection AddSbRepositoryService(this IServiceCollection services, params Type[] interceptorTypes)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(it => it.GetTypes()).ToList();

            var tableType = types.Where(it => it.GetCustomAttribute<TableAttribute>() != null);

            foreach (var type in tableType)
            {
                //var injectServiceType = typeof(IRepository<>).MakeGenericType(type);
                var serivceType = typeof(BaseRepository<>).MakeGenericType(type);
                services.AddSbScoped(serivceType, interceptorTypes);
            }

            return services;
        }
        public static IServiceCollection AddSbRepositoryService2(this IServiceCollection services, params Type[] interceptorTypes)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(it => it.GetTypes()).ToList();

            var tableType = types.Where(it => it.GetCustomAttribute<TableAttribute>() != null);

            foreach (var type in tableType)
            {
                var serviceType = typeof(IBaseRepository<>).MakeGenericType(type);
                var implementType = typeof(BaseRepository<>).MakeGenericType(type);
                services.AddSbScoped(serviceType, implementType, interceptorTypes);
            }

            return services;
        }
        /// <summary>
        /// 瞬时
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, name, interceptorTypes);
        }

        /// <summary>
        /// 瞬时,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, name, interceptorTypes);
        }

        /// <summary>
        /// 瞬时
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbTransient(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Transient, name, interceptorTypes);
        }

        /// <summary>
        /// 瞬时,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbTransient(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Transient, name, interceptorTypes);
        }

        /// <summary>
        /// 请求级别
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, name, interceptorTypes);
        }

        /// <summary>
        /// 请求级别,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, name, interceptorTypes);
        }

        /// <summary>
        /// 请求级别
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbScoped(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Scoped, name, interceptorTypes);
        }

        /// <summary>
        /// 请求级别,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbScoped(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Scoped, name, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, name, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton<TService, TImplementation>(this IServiceCollection services, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == typeof(TService))) return services;
            return services.AddSbService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, name, interceptorTypes);
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddSbSingleton(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Singleton, name, interceptorTypes);
        }

        /// <summary>
        /// 单例,如果已存在则不添加
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="name"></param>
        /// <param name="interceptorTypes"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSbSingleton(this IServiceCollection services, Type serviceType,
            Type implementationType, string name = "", params Type[] interceptorTypes)
        {
            if (services.Any(it => it.ServiceType == serviceType)) return services;
            return services.AddSbService(serviceType, implementationType, ServiceLifetime.Singleton, name, interceptorTypes);
        }

        public static IServiceCollection AddSbService(this IServiceCollection services, Type serviceType, Type implementationType,
            ServiceLifetime lifetime, string name = "", params Type[] interceptorTypes)
        {
            services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

            //添加cache拦截器
            var enableCahce = false;
            var methods = implementationType.GetTypeInfo().DeclaredMethods;
            //返回值类型列表
            //var requireTypes = new List<Type>();
            foreach (var methodInfo in methods)
            {
                if (methodInfo.GetCustomAttribute<CacheableAttribute>() != null ||
                    methodInfo.GetCustomAttributes<CachePutAttribute>().Any() ||
                    methodInfo.GetCustomAttributes<CacheEvictAttribute>().Any())
                {
                    enableCahce = true;
                }
            }

            object Factory(IServiceProvider provider)
            {
                var target = provider.GetService(implementationType);
                var properties = implementationType.GetTypeInfo().DeclaredProperties;

                foreach (PropertyInfo info in properties)
                {
                    //属性注入
                    var autowiredAttribute = info.GetCustomAttribute<AutowiredAttribute>();
                    if (autowiredAttribute != null)
                    {
                        var isRequire = autowiredAttribute.Require;
                        var propertyType = info.PropertyType;
                        object impl = null;
                        var qualifierAttribute = info.GetCustomAttribute<QualifierAttribute>();
                        if (qualifierAttribute != null)
                        {
                            var serviceName = qualifierAttribute.Name;
                            impl = provider.GetServiceByName(serviceName, propertyType, isRequire);
                        }
                        else
                        {
                            impl = isRequire ? provider.GetRequiredService(propertyType) : provider.GetService(propertyType);
                        }

                        if (impl != null)
                        {
                            info.SetValue(target, impl);
                        }
                    }

                    //配置值注入
                    if (info.GetCustomAttribute<ValueAttribute>() is ValueAttribute valueAttribute)
                    {
                        var value = valueAttribute.Value;
                        if (provider.GetService(typeof(IConfiguration)) is IConfiguration configService)
                        {
                            var pathValue = configService.GetSection(value).Value;
                            if (pathValue != null)
                            {
                                var pathV = Convert.ChangeType(pathValue, info.PropertyType);
                                info.SetValue(target, pathV);
                            }
                        }

                    }
                }

                //添加缓存拦截器
                List<IInterceptor> interceptors = interceptorTypes.ToList()
                    .ConvertAll<IInterceptor>(interceptorType => provider.GetService(interceptorType) as IInterceptor);

                if (enableCahce)
                {
                    var cacheInterceptor = provider.GetService<CacheInterceptor>();
                    interceptors.Add(cacheInterceptor);
                }

                var proxyGenerator = provider.GetService<ProxyGenerator>();
                var options = new ProxyGenerationOptions();
                options.AddMixinInstance(new DependencyAddition() { Name = name });
                var proxy = proxyGenerator.CreateInterfaceProxyWithTarget(serviceType, target, options, interceptors.ToArray());

                if (proxy is IInitializing proxyTmp)
                {
                    proxyTmp.AfterPropertiesSet(); ;
                }

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }


        /// <summary>
        /// 根据名称获得service,找不到时报错,泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetServiceByName<T>(this IServiceProvider serviceProvider, string name)
        {
            T result = default;
            var services = serviceProvider.GetServices<T>();
            var isFindService = false;
            foreach (var service in services)
            {
                if (ProxyUtil.IsProxy(service))
                {
                    var addition = service as IDependencyAddition;
                    if (addition?.Name == name)
                    {
                        result = service;
                        isFindService = true;
                        break; ;
                    }
                }
            }
            if (!isFindService) throw new Exception("找不到名称为:" + name + nameof(T) + " 的服务");

            return result;
        }

        /// <summary>
        /// 根据名称获得service,找不到时报错,非泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="name"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetServiceByName(this IServiceProvider serviceProvider, string name, Type serviceType, bool isRequire = false)
        {
            var result = new object();
            var services = serviceProvider.GetServices(serviceType);
            if (services.Count() == 0 && isRequire) throw new Exception("can not find service that type is " + serviceType.Name);

            var isFindService = false;
            foreach (var service in services)
            {
                if (ProxyUtil.IsProxy(service))
                {
                    var addition = service as IDependencyAddition;
                    if (addition?.Name == name)
                    {
                        result = service;
                        isFindService = true;
                        break; ;
                    }
                }
            }
            if (!isFindService) throw new Exception("cannt find service of name:" + name + serviceType.Name);

            return result;
        }

        public static IServiceCollection AddSbRepositoryService(this IServiceCollection services, Type serviceType,
            ServiceLifetime lifetime)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            var returnTypes = serviceType?.GetTypeInfo().DeclaredMethods?.Select(it => it.ReturnType).ToList();

            var returnModels = returnTypes?.Select(it => it.GetUnderlyingType()).Distinct().ToList();

            if (returnModels == null) return services;

            foreach (Type model in returnModels)
            {
                //var requireType = typeof(RepositoryInterceptor<>).MakeGenericType(model);
                //services.AddSbScoped(requireType);
            }

            object Factory(IServiceProvider provider)
            {
                var interceptors = new List<IInterceptor>();
                foreach (Type model in returnModels)
                {
                    //var requireType = typeof(RepositoryInterceptor<>).MakeGenericType(model);
                    //var repositoryInterceptor = provider.GetService(requireType);

                    //interceptors.Add((IInterceptor)repositoryInterceptor);
                }

                var proxyGenerator = provider.GetService<ProxyGenerator>();
                var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, Type.EmptyTypes, interceptors.ToArray());

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }

        public static IServiceCollection AddSummerBootCache(this IServiceCollection services, Action<SbCacheOption> setUpOption)
        {
            if (setUpOption == null)
            {
                throw new ArgumentNullException(nameof(setUpOption));
            }

            services.AddSbScoped<CacheInterceptor>();

            var option = new SbCacheOption();
            setUpOption(option);

            if (option.IsUseRedis)
            {
                if (option.RedisConnectionStr.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("redis connection string must not be empty");
                }

                services.AddSbSingleton<IRedisCacheWriter, RedisCacheWriter>();
                services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>((t) => ConnectionMultiplexer.Connect(option.RedisConnectionStr)
         );
                //services.AddSbScoped<ICache, RedisCache>();
                services.AddSbScoped<ICacheManager, RedisCacheManager>("redis");
            }
            //如果未添加序列化器，则默认序列化器为json

            services.TryAddSbSingleton<ISerialization, JsonSerialization>();
            return services;
        }

        public static IServiceCollection AddSummerBoot(this IServiceCollection services, CultureInfo cultureInfo = null)
        {
            //设置语言
            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentCulture;
            ResourceManager.cultureInfo = cultureInfo;
            //var f= ResourceManager.InternalGet("err1");
            //Console.WriteLine("进入多语言模式"+f);
            services.AddSingleton<ProxyGenerator>();
            services.AddLogging();
            services.AddSbScoped<TransactionalInterceptor>();
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

            var feignTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<FeignClientAttribute>() != null);

            foreach (var type in feignTypes)
            {
                CheckFeignFallBack(type, services);
                services.AddSummerBootFeignService(type, ServiceLifetime.Scoped);
            }
            return services;
        }

        private static void CheckFeignFallBack(Type type, IServiceCollection services)
        {
            var feignClient = type.GetCustomAttribute<FeignClientAttribute>();
            var fallBack = feignClient.FallBack;
            if (fallBack != null)
            {
                if (!fallBack.GetInterfaces().Contains(type)) throw new Exception("fallback must implement " + type.Name);
                services.AddSbScoped(type, fallBack, type.Name + "FallBack");
            }
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

                //var interceptors = new List<IInterceptor>();
                //var feignInterceptor = provider.GetService<FeignInterceptor>();
                //interceptors.Add(feignInterceptor);
                //provider.GetService<IProxyBuilder>();

                //var proxyGenerator = provider.GetService<ProxyGenerator>();
                //var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, Type.EmptyTypes, interceptors.ToArray());

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            services.Add(serviceDescriptor);

            return services;
        }
    }
}