using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.DbExecute.Common.Repository
{
    [AutoRepository]
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
       
    }
}