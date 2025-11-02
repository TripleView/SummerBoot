using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class OracleQueryFormatter : QueryFormatter
    {
        public OracleQueryFormatter(DatabaseUnit databaseUnit) : base(":", "\"", "\"", databaseUnit)
        {

        }

        protected override string GetFunctionAlias(string functionName)
        {
            if (functionName == "LEN")
            {
                return "LENGTH";
            }
            return base.GetFunctionAlias(functionName);
        }

        //public override DbQueryResult Insert<T>(T insertEntity)
        //{
        //    var result = base.Insert(insertEntity);
        //    if (result.IdKeyPropertyInfo != null)
        //    {

        //        result.Sql += $" RETURNING {BoxColumnName(result.IdName)} INTO {parameterPrefix}{result.IdName}";
        //    }

        //    return result;
        //}

        public override DbQueryResult FastBatchInsert<T>(List<T> insertEntitys)
        {
            var table = this.GetTableInfo(typeof(T));
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

            var allCount = insertEntitys.Count;

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
                        dbType = DbType.Byte;
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
                foreach (var insertEntity in insertEntitys)
                {
                    var propertyValue = insertEntity.GetPropertyValue(propertyName);
                    array!.SetValue(propertyValue, k);
                    k++;
                }

                //this.dynamicParameters.Add(new SqlParameter() { Value = array, DbType = dbType });
            }

            _sb.Append($"insert into {tableName} ({string.Join(",", columnNameList)}) values ({string.Join(",", parameterNameList)})");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                DynamicParameters = this.dynamicParameters,
            };

            return result;
        }
    }
}