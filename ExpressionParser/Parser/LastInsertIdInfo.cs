using System.Reflection;

namespace ExpressionParser.Parser
{
    public class LastInsertIdInfo
    {
        public string LastInsertIdSql { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}