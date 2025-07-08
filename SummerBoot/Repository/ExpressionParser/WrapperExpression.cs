using System.Linq.Expressions;
using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.ExpressionParser;

public class WrapperExpression:Expression
{
    public SqlExpression SqlExpression { get; set; }
}