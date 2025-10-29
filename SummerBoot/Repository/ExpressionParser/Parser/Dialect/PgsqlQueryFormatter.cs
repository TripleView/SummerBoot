using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class PgsqlQueryFormatter : QueryFormatter
    {
        public PgsqlQueryFormatter(DatabaseUnit databaseUnit):base("@", "\"","\"",databaseUnit)
        {
            
        }

        public override DbQueryResult Insert<T>(T insertEntity)
        {
            var result = base.Insert(insertEntity);
            if (result.IdKeyPropertyInfo != null)
            {

                result.Sql += $" RETURNING {BoxColumnName(result.IdName)}";
            }

            return result;
        }

        protected override string GetFunctionAlias(string functionName)
        {
            if (functionName == "LEN")
            {
                return "LENGTH";
            }
            return base.GetFunctionAlias(functionName);
        }


    }
}