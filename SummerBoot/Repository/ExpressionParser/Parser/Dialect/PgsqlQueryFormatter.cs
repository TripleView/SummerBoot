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

    }
}