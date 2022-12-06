using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface ICustomerWithSchemaRepository : IBaseRepository<CustomerWithSchema>
    {
        
    }
    [OracleAutoRepository]
    public interface ICustomerWithSchema2Repository : IBaseRepository<CustomerWithSchema2>
    {
        [Select("select * from TEST1.CUSTOMERWITHSCHEMA order by age")]
        Page<CustomerWithSchema2> TestPage(IPageable page);
    }
}