using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("customer")]
    public class Customer
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity),Column("id")]
        public int Id { set; get; }
        [Column("name")]
        public string Name { set; get; }
        [Column("age")]
        public int Age { set; get; } = 0;
        /// <summary>
        /// 会员号
        /// </summary>
        [Column("customerno")]
        public string CustomerNo { set; get; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        [Column("totalconsumptionamount")]
        public decimal TotalConsumptionAmount { set; get; }
    }
}
