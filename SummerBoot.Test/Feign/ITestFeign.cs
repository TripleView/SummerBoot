using System.Threading.Tasks;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Test.Feign
{
    public class test
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class poco
    {
        [AliasAs(Name = "methodName")]
        public string Name { get; set; }
    }
    [FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor))]
    public interface ITestFeign
    {
        [IgnoreInterceptor]
        [PostMapping("/form")]
        Task<dynamic> Test([Body(BodySerializationKind.Form)] test tt);
        [PostMapping("/{methodName}")]
        Task<dynamic> Test2([Body(BodySerializationKind.Form)] test tt, poco poco);

        [Multipart]
        [PostMapping("/multipart")]
        Task<dynamic> MultipartTest([Body(BodySerializationKind.Form)] test tt, MultipartItem item);

        [PostMapping("/form")]
        Task<dynamic> TestHeaderColloction([Body(BodySerializationKind.Form)] test tt, HeaderCollection pp );
    }
}