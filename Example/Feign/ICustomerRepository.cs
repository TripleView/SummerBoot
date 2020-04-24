using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Feign;

namespace Example.Feign
{
    /// <summary>
    /// 会员接口
    /// </summary>
    [FeignClient(name: "CustomerService", url: "http://localhost:5001/", fallBack: typeof(CustomerFallBack))]
    [Polly(retry:3,timeout:2000,retryInterval:1000)]
    public interface ICustomerRepository
    {
        /// <summary>
        /// 更新会员总消费金额
        /// </summary>
        /// <param name="customerNo"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [PostMapping("/Customer/UpdateCustomerAmount")]
        [Headers("Content-Type:application/json", "Accept:application/json", "Charset:utf-8")]
        Task<bool> UpdateCustomerAmount(string customerNo, decimal amount);
    }
}
