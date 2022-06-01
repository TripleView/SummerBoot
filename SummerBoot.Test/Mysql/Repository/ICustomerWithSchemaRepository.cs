using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
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
        [Select("select * from test.CustomerWithSchema order by age")]
        Page<Oracle.Models.CustomerWithSchema2> TestPage(IPageable page);
    }
}