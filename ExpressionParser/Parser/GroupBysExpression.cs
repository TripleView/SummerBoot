using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// 多个group by表达式
    /// </summary>
    public class GroupBysExpression : Expression
    {
        public GroupBysExpression(List<GroupByExpression> groupByExpressions)
        {
            GroupByExpressions = groupByExpressions;
        }
        public List<GroupByExpression> GroupByExpressions { get; set; }
    }
}