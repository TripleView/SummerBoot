using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.Oracle.Models
{
    public class SpecifiedMapTestTable
    {
        public string NormalTxt { get; set; }
        /// <summary>
        /// 特殊指定的类型
        /// </summary>
        [MappingToDatabaseType("text")]
        public string SpecifiedTxt { get; set; }
    }
}