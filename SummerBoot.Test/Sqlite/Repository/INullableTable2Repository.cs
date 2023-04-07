using SummerBoot.Repository;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    [SqliteAutoRepositoryAttribute]
    public interface INullableTable2Repository: IBaseRepository<NullableTable2>
    {
        
    }
}