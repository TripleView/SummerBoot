namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class SqliteQueryFormatter : QueryFormatter
    {
        public SqliteQueryFormatter(DatabaseUnit databaseUnit):base(":", "`","`",databaseUnit)
        {
            
        }
    }
}