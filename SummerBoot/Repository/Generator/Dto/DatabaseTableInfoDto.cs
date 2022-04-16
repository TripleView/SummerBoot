using System.Collections.Generic;

namespace SummerBoot.Repository.Generator.Dto
{
    /// <summary>
    /// 数据库表信息
    /// </summary>
    public class DatabaseTableInfoDto
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 表描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 字段信息
        /// </summary>
        public List<DatabaseFieldInfoDto> FieldInfos { get; set; }
    }
}