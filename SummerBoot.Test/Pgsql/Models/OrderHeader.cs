using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("orderheader")]
    public class OrderHeader
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public int Id { set; get; }
        [Column("customerid")]
        public int CustomerId { set; get; }
        [Column("createtime")]
        public DateTime CreateTime { set; get; }
        [Column("orderno")]
        public string OrderNo { set; get; }
        [Column("state")]
        public int State { set; get; }
    }
}
