using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Feign;

namespace Example.Feign
{
    [FeignClient(name: "queryUser", url: "http://localhost:5242/", fallBack: typeof(QueryEmployeeFallBack))]
    [Polly(retry:3,openCircuitBreaker:true,exceptionsAllowedBeforeBreaking:1, durationOfBreak: 5000,timeout:2000,retryInterval:1000)]
    public interface IQueryEmployee
    {
        [PostMapping("/home/test")]
        [Headers("Content-Type:application/json", "Accept:application/json", "Charset:utf-8")]
        Task<Employee> FindAsync([Param("p")]string id, [Body] Employee user);

        [GetMapping("")]
        Task<List<Employee>> GetEmployeeAsync(string url, int ab);
        
        [GetMapping("/home/getCount")]
        Task<int> GetEmployeeCountAsync();
    }
}
