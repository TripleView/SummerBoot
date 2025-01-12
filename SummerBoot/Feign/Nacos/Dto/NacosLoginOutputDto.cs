namespace SummerBoot.Feign.Nacos.Dto;

public class NacosLoginOutputDto
{
    public string AccessToken { get; set; }

    public long TokenTtl { get; set; }

}