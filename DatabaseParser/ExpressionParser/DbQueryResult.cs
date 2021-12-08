using System.Collections.Generic;

namespace DatabaseParser.ExpressionParser
{
    public class DbQueryResult
    {
        public string Sql { get; set; }
        public List<SqlParameter> SqlParameters { get; set; }
    }
}