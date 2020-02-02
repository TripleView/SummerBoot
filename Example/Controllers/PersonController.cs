using System.Net.Http;
using Example.Models;
using Example.Service;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using System.Threading.Tasks;
using Example.Feign;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        [Autowired]
        private IPersonService PersonService { set; get; }
        [Autowired]
        private IQueryUser QueryUser { set; get; }

        [Autowired]
        private IHttpClientFactory IHttpClientFactory { set; get; }
        //[HttpGet("testFeign")]
        //public async Task<IActionResult> TestFeign()
        //{
        //    var f =await QueryUser.FindAsync("666",new User(){Name = "summer",Value = "boot"});
        //   return Ok(f);
        //}

        [HttpGet("testFeign")]
        public IActionResult TestFeign()
        {
            var f =  QueryUser.Find("666", new User() { Name = "summer", Value = "boot" });
            return Ok(f);
        }
        [HttpGet("testFeign1")]
        public async Task<IActionResult> TestFeign2()
        {
            var client= IHttpClientFactory.CreateClient();
            var httpContent=new StringContent(new User(){Name = "123",Value = "456"}.ToJson());
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var f=await client.PostAsync("http://localhost:6296/WeatherForecast/Find2?Id=3223", httpContent);
            //var f =await QueryUser.FindAsync("666", new User() { Name = "summer", Value = "boot" });
            return Ok(f);
        }
        [HttpGet("InsertPerson")]
        public IActionResult InsertPerson()
        {
            var person=new Person(){Age = RandonDataService.GetRandomNumber(1,99),Name = RandonDataService.GetRandomName()};
            var result=PersonService.InsertPerson(person);
            return Ok(result);
        }

        [HttpGet("FindPerson")]
        public IActionResult FindPerson()
        {
            var person = PersonService.FindPerson("喻星玉");
            return Ok(person);
        }

        [HttpGet("UpdatePerson")]
        public IActionResult UpdatePerson()
        {
            var person = PersonService.UpdatePerson(new Person());
            return Ok(person);
        }

        [HttpGet("DeletePerson")]
        public IActionResult DeletePerson()
        {
            var success = PersonService.DeletePerson(new Person());
            return Ok(success);
        }

        [HttpGet("FindPersonAsync")]
        public async Task<IActionResult> FindPersonAsync()
        {
            var person = await PersonService.FindPersonAsync("野猪佩奇");
            return Ok(person);
        }

        [HttpGet("UpdatePersonAsync")]
        public async Task<IActionResult> UpdatePersonAsync()
        {
            var person = await PersonService.UpdatePersonAsync(new Person());
            return Ok(person);
        }

        [HttpGet("DeletePersonAsync")]
        public async Task<IActionResult> DeletePersonAsync()
        {
            var success = await PersonService.DeletePersonAsync(new Person());
            return Ok(success);
        }
    }
}