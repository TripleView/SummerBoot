using System;

namespace SummerBoot.Feign.Attributes
{
    /// <summary>
    /// 忽略拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreInterceptorAttribute : Attribute
    {

    }
}