using System.Collections.Generic;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    /// <summary>
    /// 获取数据库信息
    /// </summary>
    public interface IDatabaseInfo
    {
        /// <summary>
        /// 获取数据库表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        DatabaseTableInfoDto GetTableInfoByName(string tableName);
        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo);
    }
}