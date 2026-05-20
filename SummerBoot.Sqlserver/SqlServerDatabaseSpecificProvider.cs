using Microsoft.Data.SqlClient;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.SqlServer;

public class SqlServerDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public SqlServerDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        this.OpenDb();
        var (sqlBulkCopy, insertData) = GetCommand(list);
        sqlBulkCopy.WriteToServer(insertData);
        this.CloseDb();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
        this.OpenDb();
        var (sqlBulkCopy, insertData) = GetCommand(list);
        await sqlBulkCopy.WriteToServerAsync(insertData);
        this.CloseDb();
    }

    private (SqlBulkCopy, DataTable) GetCommand<T>(List<T> list)
    {
        var sqlBulkCopy = this.dbTransaction == null ?
            new SqlBulkCopy((SqlConnection)this.dbConnection) :
            new SqlBulkCopy((SqlConnection)this.dbConnection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)this.dbTransaction);
        var internalResult = GetBatchQueryCondition(list);
        sqlBulkCopy.DestinationTableName = internalResult.Sql;
        sqlBulkCopy.BatchSize = 1000;
        foreach (var mapping in internalResult.PropertyInfoMappings)
        {
            sqlBulkCopy.ColumnMappings.Add(mapping.PropertyInfo.Name, mapping.ColumnName);
        }
        var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());
        return (sqlBulkCopy, insertData);
    }

    public override FastBatchQueryCondition GetBatchQueryCondition<T>(List<T> insertEntities)
    {
        var table = SbUtil.GetTableInfo(typeof(T));
        var tableName = GetSchemaTableName(table.Schema, table.Name);

        var result = new FastBatchQueryCondition()
        {
            Sql = tableName,
            PropertyInfoMappings = table.Columns.Where(it => !(it.IsKey && it.IsDatabaseGeneratedIdentity)).Select(it => new DbQueryResultPropertyInfoMapping() { ColumnName = it.Name, PropertyInfo = it.Property }).ToList()
        };

        return result;
    }
}