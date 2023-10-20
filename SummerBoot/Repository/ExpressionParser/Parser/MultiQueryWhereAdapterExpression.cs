using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class MultiQueryWhereAdapterExpression : DbBaseExpression
    {
        public Expression OnExpression { get; set; }
        public MultiQueryWhereAdapterExpression(Expression onExpression):base((ExpressionType)DbExpressionType.MultiQueryWhere, null)
        {
            this.OnExpression = onExpression;
        }
    }
}