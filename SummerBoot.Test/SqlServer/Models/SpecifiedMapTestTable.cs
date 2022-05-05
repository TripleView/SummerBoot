using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.SqlServer.Models
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