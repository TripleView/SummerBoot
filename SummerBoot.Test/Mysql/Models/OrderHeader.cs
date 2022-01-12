using System;
using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.Mysql.Models
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
