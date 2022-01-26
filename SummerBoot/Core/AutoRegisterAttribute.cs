using System;
using Microsoft.Extensions.DependencyInjection;

namespace SummerBoot.Core
{
    /// <summary>
    /// 自动注册注解，添加该注解，可自动注册服务
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        public AutoRegisterAttribute(Type interfaceType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            InterfaceType = interfaceType;
            ServiceLifetime = serviceLifetime;
        }
        public Type InterfaceType { get; set; }
        public ServiceLifetime ServiceLifetime { get; set; }
    }
}