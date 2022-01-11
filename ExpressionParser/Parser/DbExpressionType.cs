namespace ExpressionParser.Parser
{
    public enum DbExpressionType
    {
        Query = 1000,
        Select,
        Column,
        Table,
        Join,
        Where,
        WhereCondition,
        WhereTrueCondition,
        FunctionWhereCondition,
        OrderBy,
        GroupBy
    }
}