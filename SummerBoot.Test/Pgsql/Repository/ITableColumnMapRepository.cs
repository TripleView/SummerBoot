using SummerBoot.Repository;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [PgsqlAutoRepositoryAttribute]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {
        
    }
}