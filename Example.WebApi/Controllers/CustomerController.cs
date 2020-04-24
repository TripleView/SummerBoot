using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example.WebApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace Example.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        [Autowired()]
        private ICustomerRepository customerRepository { set; get; }

        /// <summary>
        /// 根据会员编号更新顾客总消费金额
        /// </summary>
        /// <param name="customerNo"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost("UpdateCustomerAmount")]
        public async Task<bool> UpdateCustomerAmount(string customerNo, decimal amount)
        {
            var customer = await customerRepository.GetCustomerByNoAsync(customerNo);
            
            if (customer == null) return false;

            customer.TotalConsumptionAmount += amount;
            customerRepository.Update(customer);
            return true;
        }
    }
}
