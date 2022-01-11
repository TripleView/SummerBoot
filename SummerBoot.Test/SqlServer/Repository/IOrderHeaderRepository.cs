using SummerBoot.Repository;

namespace SummerBoot.Test.SqlServer.Repository
{
    [AutoRepository]
    public interface IOrderHeaderRepository:IBaseRepository<OrderHeader>
    {

    }
}
