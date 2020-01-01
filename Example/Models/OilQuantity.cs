using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.Models
{
    [Table("OilQuantity")]
    public class OilQuantity
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 油量
        /// </summary>
        public decimal Quantity { set; get; }
    }
}