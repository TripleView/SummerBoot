using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("ORDERHEADER")]
    public class OrderHeader
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }
        [Column("CUSTOMERID")]
        public int CustomerId { set; get; }
        [Column("CREATETIME")]
        public DateTime CreateTime { set; get; }
        [Column("ORDERNO")]
        public string OrderNo { set; get; }
        [Column("STATE")]
        public int State { set; get; }
    }
}
