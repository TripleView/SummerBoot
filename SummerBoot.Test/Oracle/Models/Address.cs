using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("ADDRESS")]
    public class Address
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }
        [Column("CUSTOMERID")]
        public int CustomerId { get; set; }
        public string City { get; set; }
    }
}

