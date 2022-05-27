using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("CUSTOMER")]
    public class TableColumnMapTable
    {
        [Column("NAME")]
        public string CustomerName { get; set; }
    }
}