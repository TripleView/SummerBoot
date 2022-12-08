using SummerBoot.Repository;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    /// <summary>
    /// 测试表名字段名映射的仓储
    /// </summary>
    [MysqlAutoRepositoryAttribute]
    public interface ITableColumnMapRepository : IBaseRepository<TableColumnMapTable>
    {
        
    }
}