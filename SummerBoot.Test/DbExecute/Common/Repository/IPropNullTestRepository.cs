using SummerBoot.Repository;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.DbExecute.Common.Repository
{
    [AutoRepository]
    public interface IPropNullTestRepository : IBaseRepository<PropNullTest>
    {

    }

    [AutoRepository]
    public interface IPropNullTestItemRepository : IBaseRepository<PropNullTestItem>
    {

    }
}