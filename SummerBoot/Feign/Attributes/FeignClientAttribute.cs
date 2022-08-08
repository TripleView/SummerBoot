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
        /// 名称
        /// </summary>
        public string Name { get;private set; }

        public void SetName(string name)
        {
            this.Name = name;
        }
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
        /// <summary>
        /// 是否微服务模式
        /// </summary>
        public bool MicroServiceMode { get; set; }
        /// <summary>
        /// 微服务的名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// nacos的命名空间,非nacos可不填写
        /// </summary>
        public string NacosNamespaceId { get; set; }
        /// <summary>
        /// nacos的分组名称,非nacos可不填写
        /// </summary>
        public string NacosGroupName { get; set; }

    }
}
