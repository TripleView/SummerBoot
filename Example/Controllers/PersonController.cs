using System;
using System.Collections.Generic;
using System.Net.Http;
using Example.Models;
using Example.Service;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using System.Threading.Tasks;
using Example.Feign;
using Example.Repository;
using Gitlab用户同步.Model;
using Gitlab用户同步.Serive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SummerBoot.Repository;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        [Autowired]
        private IPersonRepository PersonService { set; get; }

        [Autowired]
        private IEmployeeRepository EmployeeRepository { set; get; }

        [Autowired]
        private IQueryEmployee QueryUser { set; get; }

        [Autowired]
        private IAddUserService AddUserService { set; get; }

        [HttpGet("testFeign1")]
        public async Task<IActionResult> TestFeign2()
        {
            var f = await QueryUser.GetEmployeeAsync("fsdfsdf", 3);
            //var f2 = await QueryUser.FindAsync("1", new Employee() { Name = "sendTest" });
            var f3 = await QueryUser.GetEmployeeCountAsync();
            return Ok(f3);
        }
        [HttpGet("InsertPerson")]
        public async Task<IActionResult> InsertPerson()
        {
            IPageable page = new Pageable(2, 10);
            //for (int i = 0; i < 100; i++)
            //{
            //    var person = new Models.Person() { Age = RandonDataService.GetRandomNumber(1, 99), Name = RandonDataService.GetRandomName() };
            //    PersonService.Insert(person);
            //}

            //var newuser = await EmployeeRepository.GetEmployeesAsync(page);
            //var person3 = await PersonService.GetPersonsListByPageAsync(page);
            //var filePath = "e://人员名单.xlsx";
            //var personList= Excel.GetSheetValues(filePath);
            //var token = "PgNu62AmyV_xNQiyCAkh";
            //var exitUser = await AddUserService.GetAllUsers(token,10000);
            ////var newuser=await EmployeeRepository.GetEmployeesAsync();

            //foreach (var it in personList)
            //{
            //    it.force_random_password = false;
            //    it.reset_password = false;
            //    it.password = "12345678";
            //    it.skip_confirmation = true;
            //    var isExist= exitUser.Exists(t => t.UserName == it.username);
            //    if (!isExist)
            //    {
            //        var b = await AddUserService.AddUsers(token, it);
            //        Console.WriteLine(b);
            //    }
            //    else
            //    {
            //        var c = 1;
            //    }

            //    await Task.Delay(200);
            //}

            //var person = new Models.Person() { Age = RandonDataService.GetRandomNumber(1, 99), Name = RandonDataService.GetRandomName() };

            //var person1 = PersonService.Insert(person);
             var person2 = await PersonService.GetPersonsListByNameAsync();
            //var person3 = await PersonService.GetPersonsListByPageAsync(page);
            //var person4 = await PersonService.GetPersonsByIdAsync(1);
            //var person5 = PersonService.GetPersonsById(1);
            //var person4 = await EmployeeRepository.GetEmployeesAsync(page);
            //var person6 = PersonService.GetPersonsListByName();
            //var equipment = await EmployeeRepository.GetEquipmentAsync(page);
            //var equipment2 = EmployeeRepository.GetEquipment(page);
            //await
            var i=await PersonService.UpdatePerson("逗比", 1);
            return Ok(1);
        }

        //[HttpGet("FindPerson")]
        //public async Task<IActionResult> FindPerson()
        //{
        //    //var person = await PersonService.GetPersonsByNameAsync("喻星玉");
        //    var page = new Pageable();
        //    page.PageNumber = 0;
        //    page.PageSize = 10;
        //    page.Sort = Sort.Asc;
        //    var person2 = await PersonService.GetPersonsListByNameAsync(page);
        //    //var person3 = PersonService.GetPersonsByName("喻星玉");
        //    //var count1 = PersonService.GetPersonTotalCount();
        //    return Ok();
        //}

        //[HttpGet("UpdatePerson")]
        //public IActionResult UpdatePerson()
        //{
        //    var person = PersonService.UpdatePerson(new Models.Person());
        //    return Ok(person);
        //}

        //[HttpGet("DeletePerson")]
        //public IActionResult DeletePerson()
        //{
        //    var success = PersonService.DeletePerson(new Models.Person());
        //    return Ok(success);
        //}

        //[HttpGet("FindPersonAsync")]
        //public async Task<IActionResult> FindPersonAsync()
        //{
        //    var person = await PersonService.FindPersonAsync("野猪佩奇");
        //    return Ok(person);
        //}

        //[HttpGet("UpdatePersonAsync")]
        //public async Task<IActionResult> UpdatePersonAsync()
        //{
        //    var person = await PersonService.UpdatePersonAsync(new Models.Person());
        //    return Ok(person);
        //}

        //[HttpGet("DeletePersonAsync")]
        //public async Task<IActionResult> DeletePersonAsync()
        //{
        //    var success = await PersonService.DeletePersonAsync(new Models.Person());
        //    return Ok(success);
        //}
    }
}