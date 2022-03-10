using System;
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
        [AliasAs(Name = "methodName")]
        public string Name { get; set; }
    }
    [FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true)]
    public interface ITestFeign
    {
        [PostMapping("/form")]
        Task<dynamic> Test([Body(BodySerializationKind.Form)] test tt);
        [PostMapping("/{methodName}")]
        Task<dynamic> Test2([Body(BodySerializationKind.Form)] test tt, poco poco);
    }
}