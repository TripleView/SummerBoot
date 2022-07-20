using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.SqlServer.Models
{
    public class OrderDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public int OrderHeaderId { set; get; }

        public string ProductName { set; get; }
        
        public int Quantity { set; get; }

        public int State { set; get; }
    }
}
