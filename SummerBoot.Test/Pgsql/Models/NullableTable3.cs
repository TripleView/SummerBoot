using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("nullabletable")]
    [Description("NullableTable test add column")]
    public class NullableTable3 : NullableTable
    {
        [Description("test add column")]
        [Column("int3")]
        public int? int3 { get; set; }
    }

}