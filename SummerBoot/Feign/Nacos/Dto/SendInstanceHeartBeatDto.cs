using System;
using System.Collections.Generic;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    /// <summary>
    /// 发送心跳dto
    /// </summary>
    public class SendInstanceHeartBeatDto
    {
        /// <summary>
        /// 命名空间ID
        /// </summary>
        [AliasAs("namespaceId")]
        public string NamespaceId { get; set; }
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
        /// 集群名称
        /// </summary>
        [AliasAs("clusterName")]
        public string ClusterName { get; set; }
        
        /// <summary>
        /// 是否临时实例
        /// </summary>
        [AliasAs("ephemeral")]
        public bool Ephemeral { get; set; }
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

        [AliasAs("beat")]
        [Embedded]
        public SendInstanceHeartBeatInstanceInfoDto Beat { get; set; }
        
    }

    public class SendInstanceHeartBeatInstanceInfoDto
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
        /// 权重
        /// </summary>
        [AliasAs("weight")]
        public double Weight { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>
        [AliasAs("serviceName")]
        public string ServiceName { get; set; }
        /// <summary>
        /// 集群
        /// </summary>
        [AliasAs("cluster")]
        public string Cluster { get; set; }

        [AliasAs("scheduled")]
        public bool Scheduled { get; set; }
        
        [Embedded]
        [AliasAs("metadata")]
        public Dictionary<string,string> MetaData { get; set; }

    }
}