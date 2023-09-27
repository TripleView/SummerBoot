using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class JoinOnExpression : DbBaseExpression
    {
        public Expression OnExpression { get; set; }
        public JoinOnExpression(Expression onExpression):base((ExpressionType)DbExpressionType.JoinOn, null)
        {
            this.OnExpression = onExpression;
        }
    }
}