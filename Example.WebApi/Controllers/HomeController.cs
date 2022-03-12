using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("form")]
        public IActionResult form([FromForm]test t)
        {
            return Content(JsonConvert.SerializeObject(t));
        }

        [HttpPost("multipart")]
        public IActionResult multipart([FromForm] test t, [FromForm] IFormFile file)
        {
            return Content(JsonConvert.SerializeObject(t) + file.FileName);
        }
    }
}
