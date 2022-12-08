using SummerBoot.Repository;
using SummerBoot.Test.MultiDatabaseMixed.Models;

namespace SummerBoot.Test.MultiDatabaseMixed.Repository
{
    [SqlServerAutoRepository]
    public interface ISqlServerCustomerRepository : IBaseRepository<Customer>
    {
     
    }
}