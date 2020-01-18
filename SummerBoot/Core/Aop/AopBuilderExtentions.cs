using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using SummerBoot.Core.Aop.Express;
using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SqlOnline.Utils;
using StackExchange.Redis;
using SummerBoot.Core.Aop.Attribute;
using SummerBoot.Core.Aop.Hook;

namespace SummerBoot.Core.Aop
{
    public static class AopBuilderExtentions
    {
        public static AopBuilder AddSbService(this AopBuilder aopBuilder,Type serviceType, Type implementationType,
                  ServiceLifetime lifetime, ProxyGenerationOptions options = default, params Type[] interceptorTypes)
        {
            aopBuilder.Service.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

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
                            impl = isRequire
                                ? provider.GetRequiredService(propertyType)
                                : provider.GetService(propertyType);
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
                var proxy = proxyGenerator.CreateInterfaceProxyWithTarget(serviceType, target, options,
                    interceptors.ToArray());

                if (proxy is IInitializing proxyTmp)
                {
                    proxyTmp.AfterPropertiesSet();
                }

                return proxy;
            }

                  ;

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            aopBuilder.Service.Add(serviceDescriptor);
            return aopBuilder;
        }


        public static AopBuilder AddSbService(this AopBuilder aopBuilder,Type serviceType,
            ServiceLifetime lifetime, params Type[] interceptorTypes)
        {
            if (aopBuilder.Service == null)
                throw new ArgumentNullException(nameof(aopBuilder.Service));
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
                            impl = isRequire
                                ? provider.GetRequiredService(propertyType)
                                : provider.GetService(propertyType);
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
                    proxyTmp.AfterPropertiesSet();
                    ;
                }

                return proxy;
            }

            ;

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            aopBuilder.Service.Add(serviceDescriptor);
            return aopBuilder;
        }


        public static AopBuilder RegisterDefaultAttribute<TInterceptor>(this AopBuilder aopBuilder,ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            return aopBuilder.RegisterExpression(new WithinAopExpress(typeof(SummerAopAttribute)),new []{ typeof(TInterceptor)},serviceLifetime);
        }


        /// <summary>
        /// 只注册有对应该类:xxx的接口：Ixxx的类
        /// </summary>
        /// <typeparam name="TInterceptor"></typeparam>
        /// <param name="aopBuilder"></param>
        /// <param name="expression"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static AopBuilder RegisterExpression<TInterceptor>(this AopBuilder aopBuilder, IAopExpress expression, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TInterceptor : IInterceptor
        {
            return aopBuilder.RegisterExpression(expression, new[] { typeof(TInterceptor) }, serviceLifetime);
        }



        /// <summary>
        /// 只注册有对应该类:xxx的接口：Ixxx的类
        /// </summary>
        /// <param name="aopBuilder"></param>
        /// <param name="expression"></param>
        /// <param name="interceptorTypes"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static AopBuilder RegisterExpression(this AopBuilder aopBuilder,IAopExpress expression, Type[] interceptorTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            interceptorTypes.AsParallel().ForAll(t => aopBuilder.AddSbService(t, serviceLifetime));
            if (expression.aopExpressType == AopExpressType.Within)//类
            {
                var ts = aopBuilder.FilterType(expression.IsMatch).ToList();
                foreach (var t in ts)
                {
                    var it = t.GetTypeInfo().GetInterface($"I{t.Name}"); 
                    if(it==null)
                        continue;
                    aopBuilder.AddSbService(it, t, serviceLifetime, ProxyGenerationOptions.Default, interceptorTypes);
                }
            }
            else //方法
            {
                var ts = aopBuilder.FilterType(expression.IsMatch).ToList();
                var fiter = new MethodInterceptorFilter((t, m) => expression.IsMatch(m));
                var options = new ProxyGenerationOptions(fiter);
                foreach (var t in ts)
                {
                    var it = t.GetTypeInfo().GetInterface($"I{t.Name}");
                    if (it == null)
                        continue;
                    aopBuilder.AddSbService(it, t, serviceLifetime, options, interceptorTypes);
                }
            }

            return aopBuilder;
        }



        public static AopBuilder AddSbRepositoryService2(this AopBuilder aopBuilder,Type serviceType,
   ServiceLifetime lifetime)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            object Factory(IServiceProvider provider)
            {
                var interceptors = new List<IInterceptor>();
                var repositoryI = provider.GetService<RepositoryInterceptor2>();
                interceptors.Add(repositoryI);

                var proxyGenerator = provider.GetService<ProxyGenerator>();
                var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, Type.EmptyTypes, interceptors.ToArray());

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            aopBuilder.Service.Add(serviceDescriptor);
            return aopBuilder;
        }

        public static AopBuilder AddSummerBootRepository(this AopBuilder aopBuilder,Action<RepositoryOption> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var option = new RepositoryOption();

            action(option);

            if (option.ConnectionString.IsNullOrWhiteSpace()) throw new Exception("ConnectionString is Require");

            if (option.DbConnectionType == null) throw new Exception("DbConnectionType is Require");

            aopBuilder.Service.AddSingleton(t => option);

            //services.AddSbSingleton<IDataSource, DruidDataSource>();

            aopBuilder.Service.AddSbScoped<RepositoryInterceptor2>();

            aopBuilder.Service.TryAddSbScoped<IDbFactory, DbFactory>();

            var repositoryTypes = aopBuilder.FilterType(it => it.IsInterface && it.GetCustomAttribute<RepositoryAttribute>() != null);

            foreach (var type in repositoryTypes)
            {
                aopBuilder.Service.AddSbRepositoryService2(type, ServiceLifetime.Scoped);
            }

            aopBuilder.Service.AddSbRepositoryService(typeof(TransactionalInterceptor));
            return aopBuilder;
        }


        public static AopBuilder AddSbRepositoryService(this AopBuilder aopBuilder,Type serviceType,
      ServiceLifetime lifetime)
        {
            if (!serviceType.IsInterface) throw new ArgumentException(nameof(serviceType));

            var returnTypes = serviceType?.GetTypeInfo().DeclaredMethods?.Select(it => it.ReturnType).ToList();

            var returnModels = returnTypes?.Select(it => it.GetUnderlyingType()).Distinct().ToList();

            if (returnModels == null) return aopBuilder;

            foreach (Type model in returnModels)
            {
                var requireType = typeof(RepositoryInterceptor<>).MakeGenericType(model);
                aopBuilder.Service.AddSbScoped(requireType);
            }

            object Factory(IServiceProvider provider)
            {
                var interceptors = new List<IInterceptor>();
                foreach (Type model in returnModels)
                {
                    var requireType = typeof(RepositoryInterceptor<>).MakeGenericType(model);
                    var repositoryInterceptor = provider.GetService(requireType);

                    interceptors.Add((IInterceptor)repositoryInterceptor);
                }

                var proxyGenerator = provider.GetService<ProxyGenerator>();
                var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, Type.EmptyTypes, interceptors.ToArray());

                return proxy;
            };

            var serviceDescriptor = new ServiceDescriptor(serviceType, Factory, lifetime);
            aopBuilder.Service.Add(serviceDescriptor);
            return aopBuilder;
        }

        public static AopBuilder AddSummerBootCache(this AopBuilder aopBuilder,Action<SbCacheOption> setUpOption)
        {
            if (setUpOption == null)
            {
                throw new ArgumentNullException(nameof(setUpOption));
            }

            aopBuilder.Service.AddSbScoped<CacheInterceptor>();

            var option = new SbCacheOption();
            setUpOption(option);

            if (option.IsUseRedis)
            {
                if (option.RedisConnectionStr.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("redis connection string must not be empty");
                }

                aopBuilder.Service.AddSbSingleton<IRedisCacheWriter, RedisCacheWriter>();
                aopBuilder.Service.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>((t) => ConnectionMultiplexer.Connect(option.RedisConnectionStr)
         );
                //services.AddSbScoped<ICache, RedisCache>();
                aopBuilder.Service.AddSbScoped<ICacheManager, RedisCacheManager>("redis");
            }
            //如果未添加序列化器，则默认序列化器为json

            aopBuilder.Service.TryAddSbSingleton<ISerialization, JsonSerialization>();
            return aopBuilder;
        }
    }

}