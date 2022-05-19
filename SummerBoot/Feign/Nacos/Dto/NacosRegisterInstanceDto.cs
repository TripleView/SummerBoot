using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    /// <summary>
    /// 注册实例dto
    /// </summary>
    public class NacosRegisterInstanceDto
    {
        /// <summary>
        /// ip地址
        /// </summary>
        [AliasAs("ip")]
        public string Ip { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        [AliasAs("port")]
        public int Port { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>
        [AliasAs("serviceName")]
        public string ServiceName { get; set; }
        /// <summary>
        /// 命名空间ID
        /// </summary>
        [AliasAs("namespaceId")]
        public string NamespaceId { get; set; }
    }
}