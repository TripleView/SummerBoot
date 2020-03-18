using System;
using System.Collections.Generic;
using System.Net.Http;
using Example.Models;
using Example.Service;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using System.Threading.Tasks;
using Example.Feign;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        [Autowired]
        private IPersonService PersonService { set; get; }

        [Autowired]
        private IQueryEmployee QueryUser { set; get; }

        [HttpGet("testFeign1")]
        public async Task<IActionResult> TestFeign2()
        {
            //var f = await QueryUser.GetEmployeeAsync("fsdfsdf", 3);
            //var f2 = await QueryUser.FindAsync("1", new Employee() { Name = "sendTest" });
            var f3 = await QueryUser.GetEmployeeCountAsync();
            return Ok(f3);
        }
        [HttpGet("InsertPerson")]
        public IActionResult InsertPerson()
        {
            var person=new Models.Person(){ Age = RandonDataService.GetRandomNumber(1,99), Name = RandonDataService.GetRandomName()};
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
            var person = PersonService.UpdatePerson(new Models.Person());
            return Ok(person);
        }

        [HttpGet("DeletePerson")]
        public IActionResult DeletePerson()
        {
            var success = PersonService.DeletePerson(new Models.Person());
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
            var person = await PersonService.UpdatePersonAsync(new Models.Person());
            return Ok(person);
        }

        [HttpGet("DeletePersonAsync")]
        public async Task<IActionResult> DeletePersonAsync()
        {
            var success = await PersonService.DeletePersonAsync(new Models.Person());
            return Ok(success);
        }
    }
}