namespace SqlParser.Dialect
{
    public class SqliteParser : SqlParser
    {
        public SqliteParser():base(":", "`", "`")
        {
            
        }
    }
}