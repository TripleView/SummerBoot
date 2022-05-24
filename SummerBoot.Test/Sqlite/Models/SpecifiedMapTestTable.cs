using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Sqlite.Models
{
    public class SpecifiedMapTestTable
    {
        public string NormalTxt { get; set; }
        /// <summary>
        /// 特殊指定的类型
        /// </summary>
        [Column("SpecifiedTxt", TypeName = "real")]
        public string SpecifiedTxt { get; set; }
    }
}