using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Example.WebApi.Model
{
    /// <summary>
    /// 会员表
    /// </summary>
    public class Customer
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 会员姓名
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 会员号
        /// </summary>
        public string CustomerNo { set; get; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        public decimal TotalConsumptionAmount { set; get; }
    }
}
