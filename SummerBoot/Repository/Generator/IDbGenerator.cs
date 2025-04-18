using System;
using System.Collections.Generic;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public interface IDbGenerator
    {
        /// <summary>
        /// Generate sql statements for database tables through c# classes
        /// 通过c#类生成数据库表的sql语句
        /// </summary>
        /// <param name="types">c# class</param>
        /// <param name="fieldTypeMappers">Generic field type mapping;通用字段类型映射</param>
        /// <returns></returns>
        List<GenerateDatabaseSqlResult> GenerateSql(List<Type> types, Dictionary<Type, string> fieldTypeMappers = null);
        /// <summary>
        /// 执行生成数据库表的sql语句
        /// </summary>
        /// <param name="generateDatabaseSqlResult"></param>
        void ExecuteGenerateSql(GenerateDatabaseSqlResult generateDatabaseSqlResult);
        List<string> GenerateCsharpClass(List<string> tableNames, string classNameSpace, string namesapce = "");

        List<string> GetAllTableNames();

        /// <summary>
        /// Get table information by name;通过名称获取表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        DatabaseTableInfoDto GetTableInfoByName(string tableName);
    }

    public interface IDbGenerator1 : IDbGenerator { }
    public interface IDbGenerator2 : IDbGenerator { }
    public interface IDbGenerator3 : IDbGenerator { }
    public interface IDbGenerator4 : IDbGenerator { }
    public interface IDbGenerator5 : IDbGenerator { }
    public interface IDbGenerator6 : IDbGenerator { }
    public interface IDbGenerator7 : IDbGenerator { }

    public interface IDbGenerator8 : IDbGenerator { }
    public interface IDbGenerator9 : IDbGenerator { }
}