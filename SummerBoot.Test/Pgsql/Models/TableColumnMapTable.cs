using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("customer")]
    public class TableColumnMapTable
    {
        [Column("name")]
        public string CustomerName { get; set; }
    }
}