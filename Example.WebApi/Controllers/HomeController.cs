using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;

namespace Example.WebApi.Controllers
{
    public class test
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("index")]
        public IActionResult Index()
        {
            return Content("ok");
        }

        [HttpPost("query")]
        public IActionResult query([FromQuery] test t)
        {
            return Content(JsonConvert.SerializeObject(t));
        }

        [HttpGet("file")]
        public IActionResult file()
        {
            var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
            //获取文件的ContentType
            var provider = new FileExtensionContentTypeProvider();
           var fileExt= Path.GetExtension(basePath);
            var memi = provider.Mappings[fileExt];
            return File(new FileInfo(basePath).OpenRead(), "application/octet-stream", "123.txt");
        }

        [HttpPost("form")]
        public IActionResult form([FromForm] test t)
        {
            return Content(JsonConvert.SerializeObject(t));
        }

        [HttpPost("multipart")]
        public IActionResult multipart([FromForm] test t, [FromForm] IFormFile file)
        {
           
            using (FileStream stream = new FileStream(@"D:\test1\" + file.FileName, FileMode.Create))
            {
                var c= file.OpenReadStream().Length;
                Console.WriteLine($"长度为{c},是否为null{stream==null}");
                file.CopyTo(stream);
         
            }

            t.Name += "-" + file.FileName;
            return Content(JsonConvert.SerializeObject(t));
        }
    }
}
