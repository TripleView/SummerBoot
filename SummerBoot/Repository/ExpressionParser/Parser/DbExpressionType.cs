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
        JoinCondition,
        Where,
        WhereCondition,
        WhereTrueCondition,
        FunctionWhereCondition,
        OrderBy,
        GroupBy,
        MultiSelect,
        Columns,
        MultiSelectAutoFill,
        MultiQueryWhere,
        MultiQueryOrderBy
    }
}