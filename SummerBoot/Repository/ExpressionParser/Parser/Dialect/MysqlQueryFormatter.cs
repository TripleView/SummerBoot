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


        protected override string GetLastInsertIdSql()
        {
            return "Select LAST_INSERT_ID() id";
        }

        protected override string GetFunctionAlias(string functionName)
        {
            if (functionName == "LEN")
            {
                return "LENGTH";
            }
            return base.GetFunctionAlias(functionName);
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