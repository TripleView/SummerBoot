using System;
using Microsoft.Data.SqlClient;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace SummerBoot.SqlServer;

public static class RepositoryOptionExtensions
{
    public static void AddSqlServer<TUnitOfWork>(this RepositoryOption repositoryOption, string connectionString, Action<DatabaseUnit> optionAction) where TUnitOfWork : IUnitOfWork
    {
        repositoryOption.AddDatabaseUnit<SqlConnection, TUnitOfWork>(connectionString, optionAction);
        var databaseUnit = repositoryOption.DatabaseUnits[connectionString];
        databaseUnit.BindDatabaseSpecificProviderType<SqlServerDatabaseSpecificProvider>();
    }
}