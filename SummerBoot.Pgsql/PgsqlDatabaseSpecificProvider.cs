using Npgsql;
using NpgsqlTypes;
using SummerBoot.Core;
using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
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
        this.OpenDb();
        var (sql, propertyNames, propertyTypes) = GetCommand(list);
        using (var writer = await ((NpgsqlConnection)dbConnection).BeginBinaryImportAsync(sql))
        {

            foreach (var p in list)
            {
                await writer.StartRowAsync();
                for (var i = 0; i < propertyNames.Count; i++)
                {
                    var propertyName = propertyNames[i];
                    var propertyType = propertyTypes[i];
                    var value = p.GetPropertyValue(propertyName);
                    if (value is null)
                    {
                        await writer.WriteAsync(value);
                    }
                    else
                    {
                        var underlyingType = propertyType;
                        var isNullable = propertyType.IsNullable();
                        if (isNullable)
                        {
                            underlyingType = propertyType.GetUnderlyingType();
                        }

                        //如果是枚举类型，统一转为int
                        if (underlyingType.IsEnum)
                        {
                            var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);

                            value = Convert.ChangeType(value, enumUnderlyingType);
                        }
                        await writer.WriteAsync(value);
                    }
                      
                    
                }

            }
            await writer.CompleteAsync();
        }
        this.CloseDb();
    }

    private (string, List<string>, List<Type>) GetCommand<T>(List<T> list)
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

        var sql = $"copy {tableName} ({string.Join(",", columnNameList)}) from stdin (format binary)";

        return (sql, propertyNames, propertyTypes);
    }
}