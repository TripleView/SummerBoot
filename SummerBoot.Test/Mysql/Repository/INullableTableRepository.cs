using SummerBoot.Repository;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [AutoRepository]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }
}