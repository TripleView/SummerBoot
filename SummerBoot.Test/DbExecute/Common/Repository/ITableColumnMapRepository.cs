using SummerBoot.Repository;
using SummerBoot.Test.DbExecute.Common.Models;
namespace SummerBoot.Test.DbExecute.Common.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [AutoRepository]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {

    }
}