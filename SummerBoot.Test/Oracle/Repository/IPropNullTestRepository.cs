using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [OracleAutoRepository]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}