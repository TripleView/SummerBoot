using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("CUSTOMERWITHSCHEMA", Schema = "TEST1")]
    public class CustomerWithSchema
    {
        [Column("NAME")]
        public string Name { set; get; }
        [Column("AGE")]
        public int Age { set; get; } = 0;
        /// <summary>
        /// 会员号
        /// </summary>
        [Column("CUSTOMERNO")]
        public string CustomerNo { set; get; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        [Column("TOTALCONSUMPTIONAMOUNT")]
        public decimal TotalConsumptionAmount { set; get; }
    }

    [Table("CUSTOMERWITHSCHEMA", Schema = "TEST1")]
    public class CustomerWithSchema2 : OracleBaseEntity
    {
        [Column("NAME")]
        public string Name { set; get; }
        [Column("AGE")]
        public int Age { set; get; } = 0;
        /// <summary>
        /// 会员号
        /// </summary>
        [Column("CUSTOMERNO")]
        public string CustomerNo { set; get; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        [Column("TOTALCONSUMPTIONAMOUNT")]
        public decimal TotalConsumptionAmount { set; get; }
    }
}