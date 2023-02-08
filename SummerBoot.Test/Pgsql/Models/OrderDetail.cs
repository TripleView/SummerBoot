using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("orderdetail")]
    public class OrderDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public int Id { set; get; }

        [Column("orderheaderid")]
        public int OrderHeaderId { set; get; }
        [Column("productname")]
        public string ProductName { set; get; }
        [Column("quantity")]
        public int Quantity { set; get; }
        [Column("state")]
        public int State { set; get; }
    }
}
