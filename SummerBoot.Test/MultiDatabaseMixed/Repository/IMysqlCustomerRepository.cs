using SummerBoot.Repository;
using SummerBoot.Test.MultiDatabaseMixed.Models;


namespace SummerBoot.Test.MultiDatabaseMixed.Repository
{
    [AutoRepository1]
    public interface IMysqlCustomerRepository : IBaseRepository<Customer>
    {
    }
}