using SummerBoot.Feign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Feign
{
    public class QueryEmployeeFallBack : IQueryEmployee
    {
        public async Task<Employee> FindAsync([Param("")] string id, [Body] Employee user)
        {
            await Task.Delay(1000);
            return new Employee() {Name = "fallBack"};
        }

        public async Task<List<Employee>> GetEmployeeAsync(string url, int ab)
        {
            return await Task.FromResult(new List<Employee>(){new Employee(){Name = "fallBack"}});
        }

        public Task<int> GetEmployeeCountAsync()
        {
            return Task.FromResult(0);
        }
    }
}
