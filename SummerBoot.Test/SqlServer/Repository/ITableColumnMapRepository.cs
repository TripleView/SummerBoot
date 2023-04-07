using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [SqlServerAutoRepositoryAttribute]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {
        
    }
}