using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class LastInsertIdInfo
    {
        public string LastInsertIdSql { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}