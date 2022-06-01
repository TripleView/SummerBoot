using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [AutoRepository]
    public interface ICustomerWithSchemaRepository : IBaseRepository<CustomerWithSchema>
    {
        
    }
    [AutoRepository]
    public interface ICustomerWithSchema2Repository : IBaseRepository<CustomerWithSchema2>
    {
        [Select("select * from TEST1.CUSTOMERWITHSCHEMA order by age")]
        Page<CustomerWithSchema2> TestPage(IPageable page);
    }
}