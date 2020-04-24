using SummerBoot.Feign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Feign
{
    public class CustomerFallBack : ICustomerRepository
    {
        public Task<bool> UpdateCustomerAmount(string customerNo, decimal amount)
        {
            return Task.FromResult(false);
        }
    }
}
