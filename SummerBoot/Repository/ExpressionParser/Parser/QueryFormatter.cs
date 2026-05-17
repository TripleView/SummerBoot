using SqlParser.Net.Ast.Expression;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class QueryFormatter
    {

        public QueryFormatter(string parameterPrefix, string leftQuote, string rightQuote, DatabaseUnit databaseUnit)
        {
            this.parameterPrefix = parameterPrefix;
            this.leftQuote = leftQuote;
            this.rightQuote = rightQuote;
            this.databaseUnit = databaseUnit;
        }

        protected DatabaseUnit databaseUnit;

        protected readonly StringBuilder _sb = new StringBuilder();
        protected readonly StringBuilder countSqlSb = new StringBuilder();

        protected DynamicParameters dynamicParameters = new DynamicParameters();
        protected string parameterPrefix;
        protected string leftQuote;
        protected string rightQuote;

        protected virtual string GetLastInsertIdSql()
        {
            return "";
        }

        protected string BoxColumnName(string columnName)
        {
            if (columnName == "*")
            {
                return columnName;
            }

            if (databaseUnit.ColumnNameMapping != null)
            {
                columnName = databaseUnit.ColumnNameMapping(columnName);
            }

            return CombineQuoteAndName(columnName);
        }

        protected string BoxTableName(string tableName)
        {
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }
            return CombineQuoteAndName(tableName);
        }

        private string CombineQuoteAndName(string name)
        {
            return leftQuote + name + rightQuote;
        }

        protected string GetSchemaTableName(string schema, string tableName)
        {
            tableName = BoxTableName(tableName);
            tableName = schema.HasText() ? schema + "." + tableName : tableName;
            return tableName;
        }

        /// <summary>
        /// ≈˙¡ø≤Â»Î
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="insertEntitys"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual FastBatchQueryCondition FastBatchInsert<T>(List<T> insertEntitys)
        {
            throw new NotSupportedException("not support this database");
        }
    }
}