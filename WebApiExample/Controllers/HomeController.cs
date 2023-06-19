using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using WebApiExample.Model;
using WebApiExample.Repository;

namespace WebApiExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("Index")]
        public string Index()
        {
            return configuration.GetSection("a").Value+"-"+ configuration.GetSection("b").Value;
        }
    }
}