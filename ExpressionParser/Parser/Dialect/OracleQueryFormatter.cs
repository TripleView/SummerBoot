using System.Collections.Generic;

namespace ExpressionParser.Parser.Dialect
{
    public class OracleQueryFormatter : QueryFormatter
    {
        public OracleQueryFormatter():base(":","\"","\"")
        {
            
        }

        protected override string GetFunctionAlias(string functionName)
        {
            return base.GetFunctionAlias(functionName);
        }

        public override DbQueryResult Insert<T>(T insertEntity)
        {
            var result= base.Insert(insertEntity);
            if (result.IdKeyPropertyInfo != null)
            {

                result.Sql += $" RETURNING {BoxTableNameOrColumnName(result.IdName)} INTO {parameterPrefix}{result.IdName}";
            }

            return result;
        }
    }
}