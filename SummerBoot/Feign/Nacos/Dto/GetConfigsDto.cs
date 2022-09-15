using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    public class GetConfigsDto
    {
        /// <summary>
        /// 配置 ID
        /// </summary>
        [AliasAs("dataId")]
        public string DataId { get; set; }
        /// <summary>
        /// 配置分组
        /// </summary>
        [AliasAs("group")]
        public string Group { get; set; }
        /// <summary>
        /// 命名空间id
        /// </summary>
        [AliasAs("tenant")]
        public string NameSpaceId { get; set; }
    }
}