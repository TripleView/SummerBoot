using System;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// where表达式里的条件表达式，比如a=2,c>3等等
    /// </summary>
    public class WhereTwoColumnExpression : WhereExpression
    {
        public WhereTwoColumnExpression(ColumnExpression leftColumnExpression, string @operator, ColumnExpression rightColumnExpression) : base(@operator)
        {
            LeftColumnExpression = leftColumnExpression;
            RightColumnExpression = rightColumnExpression;
            NodeType = (ExpressionType)DbExpressionType.WhereTwoColumn;
        }

        public override ExpressionType NodeType { get; }
        /// <summary>
        /// 左边的列
        /// </summary>
        public ColumnExpression LeftColumnExpression { get; set; }

        /// <summary>
        /// 右边的列
        /// </summary>
        public ColumnExpression RightColumnExpression { get; set; }


    }
}