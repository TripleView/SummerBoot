using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Test.Feign
{
    public class Test
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class VariableClass
    {
        [AliasAs("methodName")]
        public string Name { get; set; }
    }

    public class VariableClass2
    {
        public string methodName { get; set; }
    }

    [FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true,
        InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]

    [Headers("a:a","b:b")]
    public interface ITestFeignWithHeader
    {
        [Headers("c:c")]
        [PostMapping("/testHeadersWithInterfaceAndMethod")]
        Task<Test> TestHeadersWithInterfaceAndMethod([Body(BodySerializationKind.Form)] Test tt);

        [PostMapping("/testHeadersWithInterface")]
        Task<Test> TestHeadersWithInterface([Body(BodySerializationKind.Form)] Test tt);
    }

    [FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
    public interface ITestFeign
    {
        [PostMapping("/json")]
        Task TestReturnTask([Body(BodySerializationKind.Json)] Test tt);
        
        [GetMapping("/downLoadWithStream")]
        Task<Stream> TestDownLoadWithStream();

        [GetMapping("/downLoadWithStream")]
        Task<HttpResponseMessage> TestDownLoadWithOriginResonse();

        [PostMapping("/form")]
        Task<HttpResponseMessage> TestOriginResponse([Body(BodySerializationKind.Form)] Test tt);

        [GetMapping("/QueryWithEscapeData")]
        Task<Test> TestQueryWithEscapeData([Query] Test tt);

        [GetMapping("/queryWithExistCondition?age=3")]
        Task<Test> TestQueryWithExistCondition([Query] string name);

        [GetMapping("/query")]
        Task<Test> TestQuery([Query] Test tt);

        [PostMapping("/form")]
        Task<Test> TestForm([Body(BodySerializationKind.Form)] Test tt);

        [PostMapping("//form")]
        Task<Test> TestUrlError([Body(BodySerializationKind.Form)] Test tt);

        [PostMapping("/testInterceptor")]
        Task<Test> TestInterceptor([Body(BodySerializationKind.Form)] Test tt);

        [IgnoreInterceptor]
        [PostMapping("/testIgnoreInterceptor")]
        Task<Test> TestIgnoreInterceptor([Body(BodySerializationKind.Form)] Test tt);

        [PostMapping("/json")]
        Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt);

        [PostMapping("/{{   methodName   }}")]
        Task<Test> TestReplaceVariableHasSpace([Body(BodySerializationKind.Form)] Test tt, string methodName);

        [PostMapping("/{{methodName}}")]
        Task<Test> TestReplaceVariableInUrlWithClass([Body(BodySerializationKind.Form)] Test tt, VariableClass2 poco);

        [PostMapping("/{{methodName}}")]
        Task<Test> TestReplaceVariableInUrlWithClassWithAlias([Body(BodySerializationKind.Form)] Test tt, VariableClass poco);

        [PostMapping("/{{methodName}}")]
        Task<Test> TestReplaceVariableInUrl([Body(BodySerializationKind.Form)] Test tt, string methodName);

        [PostMapping("/{{methodName}}")]
        Task<Test> TestReplaceVariableInUrlWithAlias([Body(BodySerializationKind.Form)] Test tt, [AliasAs("methodName")] string name);

        [Headers("a:{{methodName}}", "b:b")]
        [PostMapping("/testHeaderCollection")]
        Task<Test> TestReplaceVariableInHeaderWithClass([Body(BodySerializationKind.Form)] Test tt, VariableClass2 poco);

        [Headers("a:{{methodName}}","b:b")]
        [PostMapping("/testHeaderCollection")]
        Task<Test> TestReplaceVariableInHeaderWithClassWithAlias([Body(BodySerializationKind.Form)] Test tt, VariableClass poco);

        [Headers("a:{{methodName}}", "b:b")]
        [PostMapping("/testHeaderCollection")]
        Task<Test> TestReplaceVariableInHeader([Body(BodySerializationKind.Form)] Test tt, string methodName);

        [Headers("a:{{methodName}}", "b:b")]
        [PostMapping("/testHeaderCollection")]
        Task<Test> TestReplaceVariableInHeaderWithAlias([Body(BodySerializationKind.Form)] Test tt,[AliasAs("methodName")] string name);

        [Multipart]
        [PostMapping("/multipart")]
        Task<Test> MultipartTest([Body(BodySerializationKind.Form)] Test tt, MultipartItem item);

        [PostMapping("/testHeaderCollection")]
        Task<Test> TestHeaderCollection([Body(BodySerializationKind.Form)] Test tt, HeaderCollection headers );

        [GetMapping("/testBasicAuthorization")]
        Task<Test> TestBasicAuthorization(BasicAuthorization basicAuthorization);
        
    }
}