using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example.Dto;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace Example.Feign
{
    [FeignClient( Url = "http://localhost:5001/")]
    [Polly(retry:3,timeout:2000,retryInterval:1000)]
    public interface IFeignExampleRepository
    {
        /// <summary>
        /// post(json方式)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [PostMapping("/demo/TestJson")]
        [Headers("Content-Type:application/json", "Accept:application/json", "Charset:utf-8")]
        Task<int> TestJson(string name,[Body]AddOrderDto dto);

        /// <summary>
        /// post(form方式)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [PostMapping("/demo/TestForm")]
        Task<bool> TestForm([Body(BodySerializationKind.Form)]FeignFormDto dto);

        /// <summary>
        /// get
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        [GetMapping("/demo/GetTest")]
        Task<DateTime> TestGet(string name,int age);
    }
}
