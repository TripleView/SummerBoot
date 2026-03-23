using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.DbExecute.Common.Repository
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