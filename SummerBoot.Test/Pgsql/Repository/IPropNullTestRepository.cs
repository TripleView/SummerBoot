using SummerBoot.Repository;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{
    [PgsqlAutoRepository]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [PgsqlAutoRepository]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}