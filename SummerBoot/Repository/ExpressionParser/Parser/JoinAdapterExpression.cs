using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class JoinAdapterExpression : DbBaseExpression
    {
        public Expression OnExpression { get; set; }
        public JoinAdapterExpression(Expression onExpression):base((ExpressionType)DbExpressionType.JoinOn, null)
        {
            this.OnExpression = onExpression;
        }
    }
}