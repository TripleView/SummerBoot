using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class MysqlQueryFormatter : QueryFormatter
    {
        public MysqlQueryFormatter(DatabaseUnit databaseUnit):base("@","`","`",databaseUnit)
        {
            
        }

        public override FastBatchQueryCondition FastBatchInsert<T>(List<T> insertEntitys)
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
}