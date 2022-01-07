using SummerBoot.Repository;
using SummerBoot.Test.Models;

namespace SummerBoot.Test.Repository
{
    [AutoRepository]
    public interface IOrderHeaderRepository:IBaseRepository<OrderHeader>
    {

    }
}
