using SummerBoot.Core;
using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SummerBoot.Oracle;

public class OracleDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public OracleDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        uow.BeginTransaction();
        OpenDb();
        list.BatchExecution(OracleFastBatchInsert, 100000);
        uow.Commit();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
        uow.BeginTransaction();
        OpenDb();
        await list.BatchExecutionAsync(async (tempList) =>
        {
            await OracleFastBatchInsertAsync(tempList);
        }, 100000);
        uow.Commit();
    }

    private OracleCommand GetCommand<T>(List<T> list)
    {
        var internalResult = GetBatchQueryCondition(list);
        var cmd = (OracleCommand)dbConnection.CreateCommand();
        cmd.CommandText = internalResult.Sql;
        cmd.ArrayBindCount = list.Count;

        if (dbTransaction != null)
        {
            cmd.Transaction = dbTransaction as OracleTransaction;
        }

        foreach (var parameter in internalResult.FastBatchSqlParameters)
        {
            var param = cmd.CreateParameter();
            if (parameter.DbType == System.Data.DbType.Time)
            {
                param.OracleDbType = OracleDbType.IntervalDS;
                param.Value = parameter.Value;
            }
            else if (parameter.DbType == System.Data.DbType.DateTime)
            {
                param.OracleDbType = OracleDbType.TimeStamp;
                param.Value = parameter.Value;
                
            }
            else
            {
                param.DbType = parameter.DbType;
                param.Value = parameter.Value;
            }

            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private async Task OracleFastBatchInsertAsync<T>(List<T> list)
    {
        var cmd = GetCommand(list);
        var resultCount = await cmd.ExecuteNonQueryAsync(new CancellationToken());
    }
    private void OracleFastBatchInsert<T>(List<T> list)
    {
        var cmd = GetCommand(list);
        var resultCount = cmd.ExecuteNonQuery();
    }
    public override FastBatchQueryCondition GetBatchQueryCondition<T>(List<T> insertEntities)
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

        var allCount = insertEntities.Count;
        var fastBatchSqlParameters = new List<FastBatchSqlParameter>();
        for (int i = 0; i < propertyNames.Count; i++)
        {
            var propertyName = propertyNames[i];
            var propertyType = propertyTypes[i];

            if (databaseUnit.ParameterTypeMaps.TryGetValue(propertyType, out var dbType))
            {

            }
            else
            {
                //Determine whether it is an enumeration;ÅÐ¶ÏÊÇ·ñÎªÃ¶¾Ù 
                if (propertyType.IsEnum || (propertyType.GetUnderlyingType()?.IsEnum == true))
                {
                    dbType = System.Data.DbType.Byte;
                }
                else
                {
                    throw new NotSupportedException(propertyType.Name);
                }
            }
            //if (dbType == DbType.Time|| dbType == DbType.DateTime)
            //{
            //    continue;
            //}
            var arrayType = propertyType.MakeArrayType(1);
            var array = (Array)Activator.CreateInstance(arrayType, new object[1] { allCount });
            var k = 0;
            foreach (var insertEntity in insertEntities)
            {
                var propertyValue = insertEntity.GetPropertyValue(propertyName);
                array!.SetValue(propertyValue, k);
                k++;
            }

            fastBatchSqlParameters.Add(new FastBatchSqlParameter() { Value = array, DbType = dbType });
        }

        var sql =
            $"insert into {tableName} ({string.Join(",", columnNameList)}) values ({string.Join(",", parameterNameList)})";

        var result = new FastBatchQueryCondition()
        {
            Sql = sql.Trim(),
            FastBatchSqlParameters = fastBatchSqlParameters,
        };

        return result;
    }
}