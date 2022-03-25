using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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

        [HttpGet("file")]
        public IActionResult file()
        {
            using var d3 = testFeign.TestDownLoadStream().GetAwaiter().GetResult();
            using var newfile = new FileInfo("D:\\123.txt").OpenWrite();
            d3.CopyTo(newfile);
            return Content("ok");
        }

        [HttpGet("fileAsync")]
        public async Task<IActionResult> fileAsync()
        {
            using var streamResult =await testFeign.TestDownLoadStream();
            using var newfile = new FileInfo("D:\\123.txt").OpenWrite();
            streamResult.CopyTo(newfile);

            return Content("ok");
        }

        [HttpGet("testOriginResponse")]
        public IActionResult testOriginResponse()
        {
            using var d3 = testFeign.TestOriginResponse(new test() { Name = "hzp2", Age = 10 }).GetAwaiter().GetResult();
            var d= d3.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return Content("ok");
        }

        [HttpGet("form")]
        public IActionResult form()
        {
            var d2 = testFeign.Test2(new test() { Name = "hzp2", Age = 10 }, new poco() { Name = "form" }).GetAwaiter().GetResult();

            return Content(d2.ToString());
        }
        [HttpGet("query")]
        public IActionResult query()
        {
            var d2 = testFeign.TestQuery("何泽平").GetAwaiter().GetResult();

            return Content(d2.ToString());
        }

        [HttpGet("TestHeaderColloction")]
        public IActionResult TestHeaderColloction()
        {
            var a = new HeaderCollection
            {
                new KeyValuePair<string, string>("a", "a"),
                new KeyValuePair<string, string>("b", "b")
            };
            var d2 = testFeign.TestHeaderColloction(new test() { Name = "hzp2", Age = 10 }, a).GetAwaiter().GetResult();

            return Content(d2.ToString());
        }
        [HttpGet("auth")]
        public IActionResult auth()
        {
            var Username = "token-zpz5g";
            var Password = "l5v5r8v2j5f47cvd9lqxvhn5t4grr2np676jxxcj4bj6p4x8jx2vf4";
            var d2 = testFeign.Test2(new test() { Name = "hzp2", Age = 10 }, new poco() { Name = "form" }).GetAwaiter().GetResult();
            var d = rancherFeign.Redeploy(new BasicAuthorization(Username, Password), "c-nkv2m:p-tbtrv", "default:tt2").GetAwaiter().GetResult();

            return Content(d2.ToString());
        }
    }

}