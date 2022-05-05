using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("SPECIFIEDMAPTESTTABLE")]
    public class SpecifiedMapTestTable
    {
        [Column("NORMALTXT")]
        public string NormalTxt { get; set; }
        /// <summary>
        /// 特殊指定的类型
        /// </summary>
        [Column("SPECIFIEDTXT",TypeName = "CLOB")]
        public string SpecifiedTxt { get; set; }
    }
}