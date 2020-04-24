using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Example.Models
{
    /// <summary>
    /// 订单详情表
    /// </summary>
    public class OrderDetail
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 订单表ID
        /// </summary>
        public int OrderHeaderId { set; get; }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductNo { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { set; get; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { set; get; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { set; get; }

    }
}
