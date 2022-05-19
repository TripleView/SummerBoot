using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    /// <summary>
    /// 查询实例dto
    /// </summary>
    public class QueryInstanceListInputDto
    {
        /// <summary>
        /// 服务名
        /// </summary>
        [AliasAs("serviceName")]
        public string ServiceName { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        [AliasAs("groupName")]
        public string GroupName { get; set; }
        /// <summary>
        /// 命名空间ID
        /// </summary>
        [AliasAs("namespaceId")]
        public string NamespaceId { get; set; }
        /// <summary>
        /// 集群名称，多个集群用逗号分隔
        /// </summary>
        [AliasAs("clusters")]
        public string Clusters { get; set; }
        /// <summary>
        /// 是否只返回健康实例
        /// </summary>
        [AliasAs("healthyOnly")]
        public bool HealthyOnly { get; set; }
        

    }
}