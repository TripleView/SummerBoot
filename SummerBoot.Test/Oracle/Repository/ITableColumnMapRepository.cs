using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [AutoRepository]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {
        
    }
}