using Example.Models;
using Example.Service;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using System.Threading.Tasks;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        [Autowired]
        private IPersonService PersonService { set; get; }

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