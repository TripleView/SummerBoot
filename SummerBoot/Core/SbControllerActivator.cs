using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;

namespace SummerBoot.Core
{
    public class SbControllerActivator : IControllerActivator
    {
        /// <inheritdoc />
        public object Create(ControllerContext actionContext)
        {
            if (actionContext == null)
                throw new ArgumentNullException(nameof(actionContext));

            Type serviceType = actionContext.ActionDescriptor.ControllerTypeInfo.AsType();

            var target = actionContext.HttpContext.RequestServices.GetRequiredService(serviceType);


            var properties = serviceType.GetTypeInfo().DeclaredProperties;

            var proxyGenerator = actionContext.HttpContext.RequestServices.GetService<ProxyGenerator>();

            var proxy = proxyGenerator.CreateClassProxyWithTarget(serviceType, target);

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
                        var implList = actionContext.HttpContext.RequestServices.GetServices(propertyType);
                        if (implList.Count() == 0 && isRequire) throw new Exception("can not find serviceType of name is " + propertyType.Name);

                        foreach (var tmpImpl in implList)
                        {
                            if (ProxyUtil.IsProxy(tmpImpl))
                            {
                                var addition = tmpImpl as IDependencyAddition;
                                if (addition?.Name == serviceName)
                                {
                                    impl = tmpImpl;
                                    break; ;
                                }
                            }
                        }
                    }
                    else
                    {
                        impl = isRequire ? actionContext.HttpContext.RequestServices.GetRequiredService(propertyType) : actionContext.HttpContext.RequestServices.GetService(propertyType);
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
                    if (actionContext.HttpContext.RequestServices.GetService(typeof(IConfiguration)) is IConfiguration configService)
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
            }

            return proxy;
        }

        /// <inheritdoc />
        public virtual void Release(ControllerContext context, object controller)
        {
        }
    }
}