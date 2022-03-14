using System;
using System.Security.Policy;
using System.Threading.Tasks;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace Example.Feign
{
    [FeignClient(Url = "https://172.23.9.175:9081/",IsIgnoreHttpsCertificateValidate = true)]
    public interface IRancherFeign
    {
        [PostMapping("/v3/project/{projectId}/workloads/daemonset:{projectName}?action=redeploy")]
        Task<dynamic> Redeploy(BasicAuthorization basic,string projectId,string projectName);
    }
}