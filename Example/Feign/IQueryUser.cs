using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Feign;

namespace Example.Feign
{
    [FeignClient(name:"queryUser",url: "http://localhost:5000/",path: "home")]
    public interface IQueryUser
    {
        //[PostMapping("/find")]
        //[Headers("Content-Type:application/json", "Accept:application/json")]
        //User Find([Param]string id, [Body] User user);

        [PostMapping("/test")]
        [Headers("Content-Type:application/json", "Accept:application/json", "Charset=utf-8")]
        Task<User> FindAsync([Param]string id, [Body] User user);

        [GetMapping("/find")]
        //[Headers("Content-Type:application/json", "Accept:application/json")]
        User Find(string id, User user);
    }
}
