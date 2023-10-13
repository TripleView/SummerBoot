using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class MultiSelectExpression : DbBaseExpression
    {
        public Expression Expression { get; set; }
        public MultiSelectExpression(Expression expression):base((ExpressionType)DbExpressionType.MultiSelect, null)
        {
            this.Expression = expression;
        }
    }
}