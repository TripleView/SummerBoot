using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository;

namespace SummerBoot.Test.Mysql.Models
{
    [Table("CustomerWithSchema", Schema = "test")]
    public class CustomerWithSchema
    {
        public string Name { set; get; }
        public int Age { set; get; } = 0;
        /// <summary>
        /// 会员号
        /// </summary>
        public string CustomerNo { set; get; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        public decimal TotalConsumptionAmount { set; get; }
    }

    [Table("CustomerWithSchema", Schema = "test")]
    public class CustomerWithSchema2 : BaseEntity
    {
        public string Name { set; get; }
        public int Age { set; get; } = 0;
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