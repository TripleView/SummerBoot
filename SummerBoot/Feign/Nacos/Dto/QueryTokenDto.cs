using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    public class QueryTokenDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [AliasAs("username")]
        public string UserName { get; set; }
        /// <summary>
        /// 口令
        /// </summary>
        [AliasAs("password")]
        public string Password { get; set; }

    }
}