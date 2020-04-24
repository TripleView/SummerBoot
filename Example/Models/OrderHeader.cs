using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Example.Models
{
    /// <summary>
    /// 订单表
    /// </summary>
    public class OrderHeader
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 会员编号
        /// </summary>
        public string CustomerNo { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { set; get; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string OrderNo { set; get; }
        /// <summary>
        /// 状态 0.已取消 1.已创建 2.已支付
        /// </summary>
        public int State { set; get; }
    }
}
