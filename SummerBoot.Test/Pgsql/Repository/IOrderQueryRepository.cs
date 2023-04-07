using System.Collections.Generic;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Repository
{

    [PgsqlAutoRepositoryAttribute]
    public interface IOrderQueryRepository
    {
        [Select("select a.OrderNo,b.ProductName from OrderHeader a join OrderDetail b on a.id=b.OrderHeaderId")]
        OrderQueryDto GetOrderQuery();

        [Select("select a.OrderNo,b.ProductName from OrderHeader a join OrderDetail b on a.id=b.OrderHeaderId")]
        List<OrderQueryDto> GetOrderQueryList();
    }
}