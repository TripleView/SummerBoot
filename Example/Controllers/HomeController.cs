using Example.Feign;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Feign;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly IRancherFeign feign;
        private readonly ITestFeign testFeign;

        public HomeController(IRancherFeign feign, ITestFeign testFeign)
        {
            this.feign = feign;
            this.testFeign = testFeign;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("index")]
        public IActionResult Index()
        {
            //var Username = "token-zpz5g";
            //var Password = "l5v5r8v2j5f47cvd9lqxvhn5t4grr2np676jxxcj4bj6p4x8jx2vf4";
            //var c = "/v3/project/c-nkv2m:p-tbtrv/workloads/daemonset:default:tt2?action=redeploy";
            //var bas = new BasicAuthorization(Username,Password);
            //var d=feign.Redeploy(bas.GetBaseAuthString(), "c-nkv2m:p-tbtrv", "default:tt2").GetAwaiter().GetResult();
            //var d= testFeign.Test(new test() { Name = "hzp", Age = 10 }).GetAwaiter().GetResult();

            var d2 = testFeign.Test2(new test() { Name = "hzp2", Age = 10 },new poco(){Name = "form"}).GetAwaiter().GetResult();
            return Content("ok");
        }
    }
}