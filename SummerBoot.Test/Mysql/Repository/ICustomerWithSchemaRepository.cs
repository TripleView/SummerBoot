using SummerBoot.Repository;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [AutoRepository]
    public interface ICustomerWithSchemaRepository : IBaseRepository<CustomerWithSchema>
    {
        
    }

    [AutoRepository]
    public interface ICustomerWithSchema2Repository : IBaseRepository<CustomerWithSchema2>
    {

    }
}