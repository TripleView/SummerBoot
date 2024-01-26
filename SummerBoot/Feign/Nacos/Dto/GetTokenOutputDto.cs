using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    public class GetTokenOutputDto
    {
        /// <summary>
        /// 授权token
        /// </summary>
        [AliasAs("accessToken")]
        public string accessToken { get; set; }
        /// <summary>
        /// 有效期(秒)
        /// </summary>
        [AliasAs("tokenTtl")]
        public long tokenTtl { get; set; }


        [AliasAs("globalAdmin")]
        public bool globalAdmin { get; set; }
    }
}