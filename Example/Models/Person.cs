using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.Models
{
    [Table("person")]
    public class Person
    {
        [Key]
        public int Id { set; get; }

        public string Name { set; get; }

        public int Age { set; get; }
    }
}