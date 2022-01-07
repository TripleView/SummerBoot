using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SummerBoot.Test.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { set; get; }

        public int CustomerId { set; get; }

        public DateTime CreateTime { set; get; }

        public string OrderNo { set; get; }

        public int State { set; get; }
    }
}
