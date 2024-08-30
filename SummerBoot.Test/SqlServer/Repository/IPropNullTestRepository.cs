using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    [SqlServerAutoRepositoryAttribute]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [SqlServerAutoRepositoryAttribute]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}