using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Example.Models;
using SummerBoot.Repository;

namespace Example.Repository
{
    [Repository]
    public interface IOrderHeaderRepository:IBaseRepository<OrderHeader>
    {
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [Update("update orderHeader set state=0 where customerNo=@customerNo")]
        Task<int> CancelOrderByCustomerNoAsync(string customerNo);

        /// <summary>
        /// 删库跑路
        /// </summary>
        /// <returns></returns>
        [Delete("delete from orderHeader")]
        Task DeleteAllOrder();
    }
}
