namespace SummerBoot.Repository.ExpressionParser.Parser
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
        WhereTwoColumn,
        OrderBy,
        GroupBy,
        MultiSelect,
        Columns,
        MultiSelectAutoFill,
        MultiQueryWhere,
        MultiQueryOrderBy
    }
}