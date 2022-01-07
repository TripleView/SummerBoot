using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("CUSTOMER")]
    public class Customer
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity),Column("ID")]
        public int Id { set; get; }
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
