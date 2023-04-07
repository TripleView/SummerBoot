using SummerBoot.Feign.Attributes;

namespace WebApplication1.Feign;
//dfd8de72-e5ec-4595-91d4-49382f500edf
[FeignClient(ServiceName = "test", MicroServiceMode = true)]
public interface IFeignService
{
    [GetMapping("/home/index")]
    Task<string> TestGet();
}