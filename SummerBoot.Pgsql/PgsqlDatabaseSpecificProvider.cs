using Npgsql;
using NpgsqlTypes;
using SummerBoot.Core;
using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SummerBoot.Pgsql;

public class PgsqlDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public PgsqlDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
        var (sql, propertyTypes) = GetCommand(list);
        using (var writer = await ((NpgsqlConnection)dbConnection).BeginBinaryImportAsync(sql))
        {

            foreach (var p in list)
            {
                await writer.StartRowAsync();
                foreach (var propertyType in propertyTypes)
                {
                    await writer.WriteAsync(p.GetPropertyValue(propertyType.Name));
                }
            }
            await writer.CompleteAsync();
        }
    }

    private (string, List<Type>) GetCommand<T>(List<T> list)
    {
        var table = SbUtil.GetTableInfo(typeof(T));
        var tableName = GetSchemaTableName(table.Schema, table.Name);

        var parameterNameList = new List<string>();
        var columnNameList = new List<string>();
        var propertyNames = new List<string>();
        var propertyTypes = new List<Type>();
        var j = 1;

        foreach (var column in table.Columns)
        {

            if (column.IsKey && column.IsDatabaseGeneratedIdentity)
            {
                continue;
            }

            propertyNames.Add(column.Property.Name);
            var type = column.Property.PropertyType;
            propertyTypes.Add(type);
            var columnName = BoxColumnName(column.Name);
            columnNameList.Add(columnName);
            var parameterName = this.parameterPrefix + j;
            j++;
            parameterNameList.Add(parameterName);
        }


        var sql = $"copy {tableName} ({string.Join(",", columnNameList)}) from stdin (format binary";

        return (sql, propertyTypes);
    }
}