using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SummerBoot.Test.SqlServer.Dto
{
    public class OrderDto
    {
        public int Age { get; set; }
        public string CustomerNo { set; get; }
        public int CustomerId { set; get; }

        public DateTime CreateTime { set; get; }

        public string OrderNo { set; get; }

        public int State { set; get; }

        public string ProductName { set; get; }

        public int Quantity { set; get; }

        public string CustomerCity { get; set; }
    }

    public class OrderDto2
    {
        public int Age { get; set; }
        public string CustomerNo { set; get; }
        public int CustomerId { set; get; }

        public DateTime CreateTime { set; get; }

        public string OrderNo { set; get; }

        public int State { set; get; }

        public string ProductName { set; get; }

        public int Quantity { set; get; }

        public string CustomerCity2 { get; set; }
    }
}