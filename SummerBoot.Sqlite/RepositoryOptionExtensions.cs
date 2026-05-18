using Microsoft.Data.Sqlite;
using SummerBoot.Core;
using SummerBoot.Repository;
using System;

namespace SummerBoot.Sqlite;

public static class RepositoryOptionExtensions
{
    public static void AddSqlite<TUnitOfWork>(this RepositoryOption repositoryOption, string connectionString, Action<DatabaseUnit> optionAction) where TUnitOfWork : IUnitOfWork
    {
        repositoryOption.AddDatabaseUnit<SqliteConnection, TUnitOfWork>(connectionString, optionAction);
        var databaseUnit = repositoryOption.DatabaseUnits[connectionString];
        databaseUnit.BindDatabaseSpecificProviderType<SqliteDatabaseSpecificProvider>();
    }
}