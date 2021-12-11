using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Example.Models;
using SummerBoot.Repository;

namespace Example.Repository
{
    [Repository]
    public interface IOrderDetailRepository : IBaseRepository<OrderDetail>
    {
        /// <summary>
        /// 通过会员号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [Select("select od.* from orderHeader oh join orderDetail" +
                " od on oh.id=od.OrderHeaderId where oh.customerNo=@customerNo")]
        Task<List<OrderDetail>> GetOrderDetailByCustomerNoAsync(string customerNo);

        /// <summary>
        /// 分页,通过会员号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [Select("select od.* from orderHeader oh join orderDetail" +
                " od on oh.id=od.OrderHeaderId where oh.customerNo=@customerNo")]
        Task<Page<OrderDetail>> GetOrderDetailByCustomerNoByPageAsync(IPageable pageable,string customerNo);
    }
}
