namespace SqlParser.Dialect
{
    public class SqlServerParser:SqlParser
    {
        public SqlServerParser():base(":", "`", "`")
        {
            
        }
    }
}