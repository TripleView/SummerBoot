using SummerBoot.Repository;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [SqliteAutoRepositoryAttribute]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {
        
    }
}