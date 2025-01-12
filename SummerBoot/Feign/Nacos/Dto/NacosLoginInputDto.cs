using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    public class NacosLoginInputDto
    {
        [AliasAs("username")]
        public string UserName { get; set; }
        [AliasAs("password")]
        public string Password { get; set; }
    }
}
