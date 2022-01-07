using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("ORDERDETAIL")]
    public class OrderDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }

        [Column("ORDERHEADERID")]
        public int OrderHeaderId { set; get; }
        [Column("PRODUCTNAME")]
        public string ProductName { set; get; }
        [Column("QUANTITY")]
        public int Quantity { set; get; }
        [Column("STATE")]
        public int State { set; get; }
    }
}
