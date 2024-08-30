using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("PROPNULLTEST")]
    public class PropNullTest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }
  
        public string Name { get; set; }
    }

    public class PropNullTestItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }

        public string Name { get; set; }

        public int? MapId { get; set; }
    }
}
