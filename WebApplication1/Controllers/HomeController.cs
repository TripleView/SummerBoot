using Microsoft.AspNetCore.Mvc;
using WebApplication1.Feign;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IFeignService feignService;

        public HomeController(IFeignService feignService)
        {
            this.feignService = feignService;
        }
        [HttpGet("Index")]
        public async Task<string> Index()
        {
            var result =await feignService.TestGet();
            return result;
        }
    }
}