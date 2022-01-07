using SummerBoot.Repository;
using SummerBoot.Test.Models;

namespace SummerBoot.Test.Repository
{
    [AutoRepository]
    public interface IOrderDetailRepository:IBaseRepository<OrderDetail>
    {
    }
}
