using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Example.Feign;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Feign;

namespace Example.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly ITestFeign testFeign;
        private readonly IRancherFeign rancherFeign;

        public HomeController(ITestFeign testFeign, IRancherFeign rancherFeign)
        {
            this.testFeign = testFeign;
            this.rancherFeign = rancherFeign;
        }

        [HttpGet("index")]
        public IActionResult Index()
        {
            var d3 = testFeign.MultipartTest(new test() { Name = "hzp2", Age = 10 }, new MultipartItem(System.IO.File.OpenRead(@"D:\2.jpg"), "file", "2.jpg")).GetAwaiter().GetResult();
            return Content("ok");
        }
        [HttpGet("form")]
        public IActionResult form()
        {
            var d2 = testFeign.Test2(new test() { Name = "hzp2", Age = 10 }, new poco() { Name = "form" }).GetAwaiter().GetResult();
          
            return Content(d2.ToString());
        }

        [HttpGet("auth")]
        public IActionResult auth()
        {
            var Username = "token-zpz5g";
            var Password = "l5v5r8v2j5f47cvd9lqxvhn5t4grr2np676jxxcj4bj6p4x8jx2vf4";
            var d2 = testFeign.Test2(new test() { Name = "hzp2", Age = 10 }, new poco() { Name = "form" }).GetAwaiter().GetResult();
            var d = rancherFeign.Redeploy(new BasicAuthorization(Username,Password), "c-nkv2m:p-tbtrv", "default:tt2").GetAwaiter().GetResult();

            return Content(d2.ToString());
        }
    }

}