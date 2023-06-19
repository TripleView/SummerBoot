using System.Collections.Generic;
using SummerBoot.Core;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos
{
    /// <summary>
    /// nacos配置类
    /// </summary>
    public class NacosOption
    {
        /// <summary>
        /// 获取配置值或者默认值
        /// </summary>
        /// <returns></returns>
        public NacosOption GetConfigurationValueOrDefault()
        {
            this.NamespaceId = this.NamespaceId.GetValueOrDefault("public");
            this.GroupName = this.GroupName.GetValueOrDefault("DEFAULT_GROUP");
            this.Protocol = this.Protocol.GetValueOrDefault("http");
            this.Port = this.Port.GetValueOrDefault(80);
            this.ConfigurationOption = this.ConfigurationOption ?? new List<NacosConfigurationOption>()
            { new NacosConfigurationOption(){DataID = "", GroupName = "DEFAULT_GROUP",NamespaceId = "public"} };
            return this;
        }
        /// <summary>
        /// 当前应用支持的网络协议，http或者https
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServiceAddress { get; set; }
        /// <summary>
        /// 权重，一个服务下有多个实例，权重越高，访问到该实例的概率越大,比如有些实例所在的服务器配置高，那么权重就可以大一些，多引流到该实例
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// ip地址
        /// </summary>

        public string Ip { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>

        public int? Port { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>

        public string ServiceName { get; set; }
        /// <summary>
        /// 命名空间ID
        /// </summary>

        public string NamespaceId { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>

        public string GroupName { get; set; }
        /// <summary>
        /// 集群名
        /// </summary>

        public string ClusterName { get; set; }
        /// <summary>
        /// 是否上线
        /// </summary>

        public bool Enabled { get; set; }
        /// <summary>
        /// 是否健康
        /// </summary>

        public bool Healthy { get; set; }

        /// <summary>
        /// 是否临时实例
        /// </summary>

        public bool Ephemeral { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, string> MetaData { get; set; }
        /// <summary>
        /// nacos配置中心的设置值
        /// </summary>
        public List<NacosConfigurationOption> ConfigurationOption { get; set; }
    }

    public class NacosConfigurationOption
    {
        /// <summary>
        /// 命名空间id
        /// </summary>
        public string NamespaceId { get; set; }
        /// <summary>
        /// nacos配置的Data ID
        /// </summary>
        public string DataID { get; set; }
        /// <summary>
        /// nacos配置的分组名称
        /// </summary>
        public string GroupName { get; set; }
      
    }
}