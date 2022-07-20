using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.SqlServer.Models
{
    public class OrderHeader
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public int CustomerId { set; get; }

        public DateTime CreateTime { set; get; }

        public string OrderNo { set; get; }

        public int State { set; get; }
    }
}
