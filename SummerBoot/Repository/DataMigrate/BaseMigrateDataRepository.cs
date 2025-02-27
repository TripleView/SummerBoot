﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SummerBoot.Core;
using SummerBoot.Repository.DataMigrate;
using SummerBoot.Repository.ExpressionParser.Parser;

namespace SummerBoot.Repository.DataMigrate;

public class BaseMigrateDataRepository : CustomBaseRepository<BaseEntity>, IDataMigrateRepository
{
    private readonly IDbFactory dbFactory;
    private readonly IUnitOfWork unitOfWork;

    public BaseMigrateDataRepository(IUnitOfWork unitOfWork, IDbFactory dbFactory) : base(unitOfWork, dbFactory)
    {
        this.dbFactory = dbFactory;
        this.unitOfWork = unitOfWork;
    }
    public async Task MigratingDataWithAutoIncrementPrimaryKey<T>(Expression<Func<T, object>> selectKey, Func<Task> migrateAction)
    {
        var databaseUnit = dbFactory.DatabaseUnit;
        if (databaseUnit.IsOracle)
        {
            var tableName = "";
            var keyName = "";
            var newKeyName = "";
            if (selectKey.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpression)
            {
                var table = new TableExpression(memberExpression.Expression.Type);
                var column = table.Columns.FirstOrDefault(it => it.MemberInfo.Name == memberExpression.Member.Name);
                if (column != null)
                {
                    if (column.IsKey && column.IsDatabaseGeneratedIdentity)
                    {
                        tableName = table.Name;
                        if (databaseUnit.TableNameMapping != null)
                        {
                            tableName = databaseUnit.TableNameMapping(tableName);
                        }

                        tableName = $"\"{tableName}\"";
                        keyName = column.ColumnName;
                        if (databaseUnit.ColumnNameMapping != null)
                        {
                            keyName = databaseUnit.ColumnNameMapping(keyName);
                        }
                        newKeyName = $"\"New{keyName}\"";
                        keyName = $"\"{keyName}\"";
                    }
                    else
                    {
                        throw new NotSupportedException("Can only be a primary key and auto-increment");
                    }
                }
            }
            else
            {
                throw new NotSupportedException("selectKey not support,Can only be similar to it=>it.id");

            }

            await this.ExecuteAsync($"ALTER TABLE {tableName} MODIFY {keyName} DROP IDENTITY");
            await migrateAction();
            var maxId = await this.QueryFirstOrDefaultAsync<int?>($"select MAX({keyName})  from {tableName} f ");
            var count = await this.QueryFirstOrDefaultAsync<int?>($"select count({keyName})  from {tableName} f ");
            var startCount = 1;
            if (maxId.HasValue)
            {
                startCount = maxId.GetValueOrDefault(1) - count.GetValueOrDefault(0) + 1;
            }
            await this.ExecuteAsync($"ALTER TABLE {tableName} ADD  {newKeyName} NUMBER(10,0)   GENERATED BY DEFAULT ON NULL AS IDENTITY MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH {startCount} CACHE 20 NOORDER  NOCYCLE  NOKEEP  NOSCALE  NOT NULL ENABLE");
            await this.ExecuteAsync($"UPDATE {tableName}   SET {newKeyName}={keyName} ");
            await this.ExecuteAsync($"ALTER TABLE {tableName} DROP COLUMN  {keyName} ");
            await this.ExecuteAsync($"ALTER TABLE {tableName} RENAME COLUMN {newKeyName} TO {keyName}");

        }
        else
        {
            throw new NotSupportedException("Currently only supports oracle");
        }
    }
}
