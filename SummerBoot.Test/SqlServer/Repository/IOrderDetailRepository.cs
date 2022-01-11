using SummerBoot.Repository;

namespace SummerBoot.Test.SqlServer.Repository
{
    [AutoRepository]
    public interface IOrderDetailRepository:IBaseRepository<OrderDetail>
    {
    }
}
