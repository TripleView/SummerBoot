using System;
using System.Collections.Generic;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public interface IDbGenerator
    {
        /// <summary>
        /// 通过c#类生成数据库表的sql语句
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        List<GenerateDatabaseSqlResult> GenerateSql(List<Type> types);
        /// <summary>
        /// 执行生成数据库表的sql语句
        /// </summary>
        /// <param name="generateDatabaseSqlResult"></param>
        void ExecuteGenerateSql(GenerateDatabaseSqlResult generateDatabaseSqlResult);
        List<string> GenerateCsharpClass(List<string> tableNames, string classNameSpace);
    }
}