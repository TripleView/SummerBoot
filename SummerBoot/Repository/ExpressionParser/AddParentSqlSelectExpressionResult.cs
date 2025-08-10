using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.ExpressionParser;

public class AddParentSqlSelectExpressionResult
{
    public SqlSelectExpression SqlSelectExpression { get; set; }

    public string TableAlias { get; set; }
}