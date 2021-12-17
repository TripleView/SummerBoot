namespace ExpressionParser.Parser.Dialect
{
    public class OracleQueryFormatter : QueryFormatter
    {
        public OracleQueryFormatter():base(":","`","`")
        {
            
        }

        protected override string GetFunctionAlias(string functionName)
        {
            return base.GetFunctionAlias(functionName);
        }
    }
}