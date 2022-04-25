using System.Collections.Generic;

namespace SummerBoot.Repository.Generator.Dto
{
    /// <summary>
    /// 生成的数据库sql
    /// </summary>
    public class GenerateDatabaseSqlResult
    {
        /// <summary>
        /// 创建表的语句
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 数据库注释，包括表注释以及数据库字段注释
        /// </summary>
        public List<string> Descriptions { get; set; }
        /// <summary>
        /// 字段变更语句
        /// </summary>
        public List<string> FieldModifySqls { get; set; }
    }
}