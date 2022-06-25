using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    [AutoRepository]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {
        
    }
}