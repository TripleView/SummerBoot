using System.Reflection;

namespace DatabaseParser.ExpressionParser
{
    public class LastInsertIdInfo
    {
        public string LastInsertIdSql { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}