using System.Collections.Generic;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    /// <summary>
    /// 查询实例返回值dto
    /// </summary>
    public class QueryInstanceListOutputDto
    {
        /// <summary>
        /// 服务名
        /// </summary>
       
        public string Dom { get; set; }
        /// <summary>
        /// 缓存时间
        /// </summary>

        public int CacheMillis { get; set; }
        /// <summary>
        /// 实例列表
        /// </summary>
        public List<QueryInstanceListItemOutputDto> Hosts { get; set; }
        /// <summary>
        /// 是否使用特殊url
        /// </summary>
        public bool UseSpecifiedUrl { get; set; }
        /// <summary>
        /// 哈希值
        /// </summary>
        public string Checksum { get; set; }
        /// <summary>
        /// 最后刷新时间
        /// </summary>
        public long LastRefTime { get; set; }
        /// <summary>
        /// 环境
        /// </summary>
       
        public string Env { get; set; }
        /// <summary>
        /// 集群
        /// </summary>

        public string Clusters { get; set; }


    }

    /// <summary>
    /// 查询实例返回值子项目
    /// </summary>
    public class QueryInstanceListItemOutputDto
    {
        public bool Valid { get; set; }
        public bool Marked { get; set; }
        public string InstanceId { get; set; }

        public int Port { get; set; }

        public string Ip { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public long Weight { get; set; }
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string,string> Metadata { get; set; }
    }
}