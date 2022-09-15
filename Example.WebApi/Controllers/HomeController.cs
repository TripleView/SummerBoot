using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Example.WebApi.Model;
using Example.WebApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;

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
        private readonly ICustomerRepository customerRepository;
        private readonly IDbGenerator dbGenerator;
        private readonly IConfiguration configuration;

        public HomeController(ICustomerRepository customerRepository, IDbGenerator dbGenerator, IConfiguration configuration)
        {
            this.customerRepository = customerRepository;
            this.dbGenerator = dbGenerator;
            this.configuration = configuration;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            var results= dbGenerator.GenerateSql(new List<Type>() { typeof(Customer) });
            var generateClasses = dbGenerator.GenerateCsharpClass(new List<string>() { "Customer" }, "Test.Model");
            foreach (var databaseSqlResult in results)
            {
                dbGenerator.ExecuteGenerateSql(databaseSqlResult);
            }

            var cusotmer = new Customer()
            {
                Name = "三合",
                Age = 3,
                CustomerNo = "00001",
                Address = "福建省",
                TotalConsumptionAmount = 999
            };
            //增
            customerRepository.Insert(cusotmer);
            //改
            cusotmer.Age = 5;
            customerRepository.Update(cusotmer);
            //也可以这样改
            customerRepository.Where(it => it.Name == "三合").SetValue(it => it.Age, 6).ExecuteUpdate();
            //查
            var dbCustomer= customerRepository.FirstOrDefault(it => it.Name == "三合");
            //删
            customerRepository.Delete(dbCustomer);
            //也可以这样删
            customerRepository.Delete(it=>it.Name== "三合");

            return Content("ok");
        }

        [HttpGet("index")]
        public IActionResult Index()
        {
            return Content("ok");
        }

        [HttpGet("TestConfiguration")]
        public IActionResult TestConfiguration()
        {
            return Content(configuration["a"]);
        }
        [HttpGet("TestConfiguration2")]
        public IActionResult TestConfiguration2()
        {
            return Content(configuration["nacos:serviceAddress"]);
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
            Console.WriteLine(Request.Cookies["Test"]);
            Console.WriteLine(Request.Cookies["Test1"]);
            Console.WriteLine(Request.Cookies["Test2"]);
            Console.WriteLine(string.Join("--", Request.Headers.Values));
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
