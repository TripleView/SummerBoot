using SummerBoot.Repository;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    [SqliteAutoRepositoryAttribute]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [SqliteAutoRepositoryAttribute]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}