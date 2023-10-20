using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.Pgsql.Models
{

    public class Address
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }
        public int CustomerId { get; set; }
        public string City { get; set; }
    }
}

