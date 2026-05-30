using MySqlConnector;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;

namespace SummerBoot.Mysql;

public class MysqlDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public MysqlDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        this.OpenDb();
        var (mySqlBulkCopy, insertData) = GetCommand(list);
        mySqlBulkCopy.WriteToServer(insertData);
        this.CloseDb();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
        this.OpenDb();
        var (mySqlBulkCopy, insertData) = GetCommand(list);
        await mySqlBulkCopy.WriteToServerAsync(insertData);
        this.CloseDb();
    }

    private (MySqlBulkCopy, DataTable) GetCommand<T>(List<T> list)
    {
        var mySqlBulkCopy = new MySqlBulkCopy((MySqlConnection)this.dbConnection, (MySqlTransaction)this.dbTransaction);
        var internalResult = GetBatchQueryCondition(list);
        mySqlBulkCopy.DestinationTableName = internalResult.Sql;
        ;
        for (int i = 0; i < internalResult.PropertyInfoMappings.Count; i++)
        {
            var property = internalResult.PropertyInfoMappings[i].PropertyInfo;
            var columnName = internalResult.PropertyInfoMappings[i].ColumnName;
            MySqlBulkCopyColumnMapping mapping;
            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
            {
                if (!databaseUnit.GuidToString)
                {
                    mapping = new MySqlBulkCopyColumnMapping(i, "@tmp", $"{columnName} = unhex(@tmp)");
                }
                else
                {
                    mapping = new MySqlBulkCopyColumnMapping(i, columnName, null);
                }
            }
            else
            {
                mapping = new MySqlBulkCopyColumnMapping(i, columnName, null);
            }

            mySqlBulkCopy.ColumnMappings.Add(mapping);
        }
        var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());
        if (!databaseUnit.GuidToString)
        {
            SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(insertData, guid1 => guid1.ToByteArray());
        }

        return (mySqlBulkCopy, insertData);
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