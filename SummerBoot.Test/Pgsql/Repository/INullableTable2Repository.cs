using SummerBoot.Repository;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{
    [PgsqlAutoRepositoryAttribute]
    public interface INullableTable2Repository: IBaseRepository<NullableTable2>
    {
        
    }
}