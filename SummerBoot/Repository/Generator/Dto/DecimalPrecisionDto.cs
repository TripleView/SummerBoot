namespace SummerBoot.Repository.Generator.Dto
{
    public class DecimalPrecisionDto
    {
        /// <summary>
        /// 精确度（默认18）
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// 保留位数（默认2）
        /// </summary>
        public int Scale { get; set; }
    }
}