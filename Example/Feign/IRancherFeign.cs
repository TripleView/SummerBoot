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
        [Headers("Authorization:{baseAuth}")]
        [PostMapping("/v3/project/{projectId}/workloads/daemonset:{projectName}?action=redeploy")]
        Task<dynamic> Redeploy(string baseAuth,string projectId,string projectName);
    }
}