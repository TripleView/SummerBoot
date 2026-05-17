using SummerBoot.Repository;
using System;
using System.Data;
using Npgsql;
using SummerBoot.Core;

namespace SummerBoot.Pgsql;

public static class RepositoryOptionExtensions
{
    public static void AddPgsql<TUnitOfWork>(this RepositoryOption repositoryOption, string connectionString, Action<DatabaseUnit> optionAction) where TUnitOfWork : IUnitOfWork
    {
        repositoryOption.AddDatabaseUnit<NpgsqlConnection, TUnitOfWork>(connectionString, optionAction);
        var databaseUnit = repositoryOption.DatabaseUnits[connectionString];
        databaseUnit.BindDatabaseSpecificProviderType<PgsqlDatabaseSpecificProvider>();
    }
}