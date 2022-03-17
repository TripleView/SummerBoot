using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace Example.Feign
{
    public class test
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class poco
    {
        [AliasAs("methodName")]
        public string Name { get; set; }
    }
    [FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor))]
    public interface ITestFeign
    {
        [PostMapping("/query")]
        Task<dynamic> TestQuery([Query] string name);

        [PostMapping("/query")]
        Task<dynamic> TestQuery([Query] test tt);

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