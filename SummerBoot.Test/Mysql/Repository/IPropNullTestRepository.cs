using SummerBoot.Repository;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [MysqlAutoRepositoryAttribute]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [MysqlAutoRepositoryAttribute]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}