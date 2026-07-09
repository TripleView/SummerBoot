using SqlParser.Net.Ast.Expression;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser.Base;

/// <summary>
/// Convert a SELECT statement to a paginated query£»selectÓï¾ä×ªÎª·ÖÒ³Óï¾ä
/// </summary>
public class SqlSelectExpressionToPageOutputDto
{
    public SqlSelectExpression PageSqlSelectExpression { get; set; }
    public SqlSelectExpression CountSqlSelectExpression { get; set; }

    public DynamicParameters DynamicParameters { get; set; }
}