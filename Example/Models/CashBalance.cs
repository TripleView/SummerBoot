using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.Models
{
    [Table("CashBalance")]
    public class CashBalance
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { set; get; }
    }
}