using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Mysql.Models
{
    [Table("Customer")]
    public class TableColumnMapTable
    {
        [Column("Name")]
        public string CustomerName { get; set; }
    }
}