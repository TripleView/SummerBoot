using System;

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
        /// 列的原始类型
        /// </summary>
        public Type ColumnType { get; set; }
        /// <summary>
        /// 列数据类型
        /// </summary>
        public string ColumnDataType { get; set; }
        /// <summary>
        /// 用户直接指定的列数据类型
        /// </summary>
        public string SpecifiedColumnDataType { get; set; }
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
        /// 字符串长度
        /// </summary>
        public int? StringMaxLength { get; set; }
        /// <summary>
        /// 精度（默认18）
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// 标度（默认2）
        /// </summary>
        public int Scale { get; set; }
    }
}