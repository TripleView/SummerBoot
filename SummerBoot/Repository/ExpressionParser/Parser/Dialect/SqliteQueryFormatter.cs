namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class SqliteQueryFormatter : QueryFormatter
    {
        public SqliteQueryFormatter(DatabaseUnit databaseUnit):base(":", "`","`",databaseUnit)
        {
            
        }

        protected override string GetLastInsertIdSql()
        {
            return "SELECT last_insert_rowid() id";
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