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
        List<string> GenerateCsharpClass( List<string> tableNames, string classNameSpace, string namesapce="");

        List<string> GetAllTableNames();
    }

    public interface IDbGenerator1 : IDbGenerator{}
    public interface IDbGenerator2 : IDbGenerator { }
    public interface IDbGenerator3 : IDbGenerator { }
    public interface IDbGenerator4 : IDbGenerator { }
    public interface IDbGenerator5 : IDbGenerator { }
    public interface IDbGenerator6 : IDbGenerator { }
    public interface IDbGenerator7 : IDbGenerator { }

    public interface IDbGenerator8 : IDbGenerator { }
    public interface IDbGenerator9 : IDbGenerator { }
}