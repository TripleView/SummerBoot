using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("NULLABLETABLE")]
    [Description("NullableTable test add column")]
    public class NullableTable3 : NullableTable
    {
        [Description("test add column")]
        public int? int3 { get; set; }
    }

}