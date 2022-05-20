using System.Collections.Generic;
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
        /// <summary>
        /// 服务权重
        /// </summary>
        [AliasAs("weight")]
        public double Weight { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        [AliasAs("groupName")]
        public string GroupName { get; set; }
        /// <summary>
        /// 集群名
        /// </summary>
        [AliasAs("clusterName")]
        public string ClusterName { get; set; }
        /// <summary>
        /// 是否上线
        /// </summary>
        [AliasAs("enabled")]
        public bool Enabled { get; set; }
        /// <summary>
        /// 是否健康
        /// </summary>
        [AliasAs("healthy")]
        public bool Healthy { get; set; }

        /// <summary>
        /// 是否临时实例
        /// </summary>
        [AliasAs("ephemeral")]
        public bool Ephemeral { get; set; }

        [Embedded]
        [AliasAs("metadata")]
        public Dictionary<string, string> MetaData { get; set; }

    }
}