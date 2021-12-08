namespace DatabaseParser.ExpressionParser
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
        FunctionWhereCondition,
        OrderBy,
        GroupBy
    }
}