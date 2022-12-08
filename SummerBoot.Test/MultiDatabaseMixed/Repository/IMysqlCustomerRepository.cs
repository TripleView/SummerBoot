using SummerBoot.Repository;
using SummerBoot.Test.MultiDatabaseMixed.Models;


namespace SummerBoot.Test.MultiDatabaseMixed.Repository
{
    [MysqlAutoRepository]
    public interface IMysqlCustomerRepository : IBaseRepository<Customer>
    {
    }
}