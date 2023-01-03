using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiExample.Model
{
    public class Customer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        /// <summary>
        /// 姓名
        /// </summary>
        [Description("姓名")]
        public string Name { set; get; }
        /// <summary>
        /// 年龄
        /// </summary>
        [Description("年龄")]
        public int Age { set; get; }

        /// <summary>
        /// 会员号
        /// </summary>
        [Description("会员号")]
        public string CustomerNo { set; get; }

        /// <summary>
        /// 总消费金额
        /// </summary>
        [Description("总消费金额")]
        public decimal TotalConsumptionAmount { set; get; }
        /// <summary>
        /// 地址
        /// </summary>
        [Description("地址")]
        public string Address { set; get; }
    }
}