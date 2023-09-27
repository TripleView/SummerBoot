namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public enum DbExpressionType
    {
        Query = 1000,
        Select,
        Column,
        Table,
        Join,
        JoinOn,
        Where,
        WhereCondition,
        WhereTrueCondition,
        FunctionWhereCondition,
        OrderBy,
        GroupBy
    }
}