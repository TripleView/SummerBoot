using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class SqlServerQueryFormatter : QueryFormatter
    {
        public SqlServerQueryFormatter(DatabaseUnit databaseUnit) : base("@", "[", "]",databaseUnit)
        {

        }

        public override DbQueryResult FastBatchInsert<T>(List<T> insertEntitys)
        {
           
            var table = this.GetTableInfo(typeof(T));
            var tableName = GetSchemaTableName(table.Schema, table.Name);

            var result = new DbQueryResult()
            {
                Sql = tableName,
                DynamicParameters = this.dynamicParameters,
                PropertyInfoMappings = table.Columns.Where(it => !(it.IsKey && it.IsDatabaseGeneratedIdentity)).Select(it => new DbQueryResultPropertyInfoMapping() { ColumnName = it.Name, PropertyInfo = it.Property }).ToList()
            };

            return result;
        }
    }
}