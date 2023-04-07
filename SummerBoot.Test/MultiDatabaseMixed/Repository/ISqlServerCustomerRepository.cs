using SummerBoot.Repository;
using SummerBoot.Test.MultiDatabaseMixed.Models;

namespace SummerBoot.Test.MultiDatabaseMixed.Repository
{
    [AutoRepository2]
    public interface ISqlServerCustomerRepository : IBaseRepository<Customer>
    {
     
    }
}