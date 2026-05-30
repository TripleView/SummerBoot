using System;
using MySqlConnector;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.TypeHandler.Dialect.Mysql;

namespace SummerBoot.Mysql;

public static class RepositoryOptionExtensions
{
    public static void AddMysql<TUnitOfWork>(this RepositoryOption repositoryOption, string connectionString, Action<DatabaseUnit> optionAction) where TUnitOfWork : IUnitOfWork
    {
        repositoryOption.AddDatabaseUnit<MySqlConnection, TUnitOfWork>(connectionString, optionAction);
        var databaseUnit = repositoryOption.DatabaseUnits[connectionString];
        databaseUnit.BindDatabaseSpecificProviderType<MysqlDatabaseSpecificProvider>();
        if (databaseUnit.GuidToString)
        {
            databaseUnit.SetTypeHandler(typeof(Guid), new MysqlStringGuidTypeHandler());
            databaseUnit.SetCsharpTypeToDatabaseTypeNameMap(typeof(Guid), "char(36)");
        }
    }
}