using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Mysql.Models
{
    public class SpecifiedMapTestTable
    {
        public string NormalTxt { get; set; }
        /// <summary>
        /// 特殊指定的类型
        /// </summary>
        [Column("SpecifiedTxt", TypeName = "text")]
        public string SpecifiedTxt { get; set; }
    }
}