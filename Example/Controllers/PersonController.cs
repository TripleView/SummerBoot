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
            return Ok(1);
        }

    }
}