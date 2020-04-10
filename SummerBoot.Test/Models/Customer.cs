using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SummerBoot.Test.Models
{
    public class Customer
    {
        [Key]
        public int Id { set; get; }
        public string Name { set; get; }
        public int Age { set; get; } = 0;
    }
}
