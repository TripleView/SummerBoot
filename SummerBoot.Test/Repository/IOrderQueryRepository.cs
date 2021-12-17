using System.Collections.Generic;
using SummerBoot.Repository;
using SummerBoot.WebApi.Models;

namespace SummerBoot.Test.Repository
{

    [AutoRepository]
    public interface IOrderQueryRepository
    {
        [Select("select a.OrderNo,b.ProductName from OrderHeader a join OrderDetail b on a.id=b.OrderHeaderId")]
        OrderQueryDto GetOrderQuery();

        [Select("select a.OrderNo,b.ProductName from OrderHeader a join OrderDetail b on a.id=b.OrderHeaderId")]
        List<OrderQueryDto> GetOrderQueryList();
    }
}