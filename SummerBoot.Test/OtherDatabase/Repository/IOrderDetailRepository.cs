using SummerBoot.Repository;
using SummerBoot.Test.OtherDatabase.Models;

namespace SummerBoot.Test.OtherDatabase.Repository
{
    [AutoRepository]
    public interface IOrderDetailRepository:IBaseRepository<OrderDetail>
    {
    }
}
