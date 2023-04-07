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
        [HttpGet("Index")]
        public string Index()
        {
            return "ok";
        }
    }
}