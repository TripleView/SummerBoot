using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    [SqlServerAutoRepositoryAttribute]
    public interface INullableTable2Repository: IBaseRepository<NullableTable2>
    {
        
    }
}