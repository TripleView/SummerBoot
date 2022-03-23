using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SummerBoot.Feign.Attributes
{
    /// <summary>
    /// feign interface attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class FeignClientAttribute : Attribute
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// url地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 回退类
        /// </summary>
        public Type FallBack { get; set; }
        /// <summary>
        /// ignore https certificate Validate； 忽略https证书校验
        /// </summary>
        public bool IsIgnoreHttpsCertificateValidate { set; get; }
        /// <summary>
        /// http request timeout，unit second， http请求超时时间，单位秒
        /// </summary>
        public int Timeout { set; get; }
        /// <summary>
        /// 定义拦截器
        /// </summary>
        public Type InterceptorType { set; get; }
    }
}
