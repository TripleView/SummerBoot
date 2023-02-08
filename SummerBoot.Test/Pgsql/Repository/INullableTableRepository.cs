using SummerBoot.Repository;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{
    [PgsqlAutoRepositoryAttribute]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }
}