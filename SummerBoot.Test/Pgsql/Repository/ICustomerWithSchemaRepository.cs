using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{
    [PgsqlAutoRepositoryAttribute]
    public interface ICustomerWithSchemaRepository : IBaseRepository<CustomerWithSchema>
    {
        
    }

    [PgsqlAutoRepositoryAttribute]
    public interface ICustomerWithSchema2Repository : IBaseRepository<CustomerWithSchema2>
    {
        [Select("select * from test1.customerwithschema order by age")]
        Page<CustomerWithSchema2> TestPage(IPageable page);
    }
}