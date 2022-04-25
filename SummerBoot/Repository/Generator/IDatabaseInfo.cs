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
        /// <summary>
        /// 添加表注释
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        string CreateTableDescription(string schema,string tableName, string description);
        /// <summary>
        /// 更新表注释
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        string UpdateTableDescription(string schema,string tableName, string description);
        /// <summary>
        /// 添加字段注释
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">字段名</param>
        /// <param name="description">注释</param>
        /// <returns></returns>
        string CreateTableFieldDescription(string schema,string tableName,string columnName, string description);
        /// <summary>
        /// 创建字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        string CreateTableField(string tableName, DatabaseFieldInfoDto fieldInfo);
    }
}