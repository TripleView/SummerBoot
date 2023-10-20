using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class MultiQueryOrderByAdapterExpression : DbBaseExpression
    {
        public Expression OnExpression { get; set; }
        public MultiQueryOrderByAdapterExpression(Expression onExpression) : base((ExpressionType)DbExpressionType.MultiQueryOrderBy, null)
        {
            this.OnExpression = onExpression;
        }
    }
}

