using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.Sqlite.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { set; get; }

        public int OrderHeaderId { set; get; }

        public string ProductName { set; get; }
        
        public int Quantity { set; get; }

        public int State { set; get; }
    }
}
