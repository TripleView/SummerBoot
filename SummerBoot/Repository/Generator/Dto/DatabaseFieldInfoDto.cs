namespace SummerBoot.Repository.Generator.Dto
{
    /// <summary>
    /// 数据库表信息
    /// </summary>
    public class DatabaseFieldInfoDto
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 列数据类型
        /// </summary>
        public string ColumnDataType { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsKey { get; set; }
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsAutoCreate { get; set; }
        /// <summary>
        /// 字段注释
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int? StringMaxLength { get; set; }
        /// <summary>
        /// decimal精度控制
        /// </summary>
        public DecimalPrecisionDto DecimalPrecision { get; set; }    
    }
}