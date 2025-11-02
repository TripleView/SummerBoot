using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("customerwithschema", Schema = "test1")]
    public class CustomerWithSchema
    {
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

    [Table("customerwithschema", Schema = "test1")]
    public class CustomerWithSchema2 : BaseEntity
    {
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