using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SummerBoot.Test.Models
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
