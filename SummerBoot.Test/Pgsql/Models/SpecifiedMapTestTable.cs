using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("specifiedmaptesttable")]
    public class SpecifiedMapTestTable
    {
        [Column("normaltxt")]
        public string NormalTxt { get; set; }
        /// <summary>
        /// 特殊指定的类型
        /// </summary>
        [Column("specifiedtxt",TypeName = "CLOB")]
        public string SpecifiedTxt { get; set; }
    }
}