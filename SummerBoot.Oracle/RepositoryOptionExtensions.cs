using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using SummerBoot.Repository;
using System;

namespace SummerBoot.Oracle;

public static class RepositoryOptionExtensions
{
    public static void AddOracle<TUnitOfWork>(this RepositoryOption repositoryOption, string connectionString, Action<DatabaseUnit> optionAction) where TUnitOfWork : IUnitOfWork
    {
        repositoryOption.AddDatabaseUnit<OracleConnection, TUnitOfWork>(connectionString, optionAction);
        var databaseUnit = repositoryOption.DatabaseUnits[connectionString];
        databaseUnit.BindDatabaseSpecificProviderType<OracleDatabaseSpecificProvider>();
    }
}