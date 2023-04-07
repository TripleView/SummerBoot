using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [MysqlAutoRepositoryAttribute]
    public interface ICustomerWithSchemaRepository : IBaseRepository<CustomerWithSchema>
    {
        
    }

    [MysqlAutoRepositoryAttribute]
    public interface ICustomerWithSchema2Repository : IBaseRepository<CustomerWithSchema2>
    {
        [Select("select * from test.CustomerWithSchema order by age")]
        Page<Oracle.Models.CustomerWithSchema2> TestPage(IPageable page);
    }
}