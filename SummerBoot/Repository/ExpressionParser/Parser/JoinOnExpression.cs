using System;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Join on表达式
    /// </summary>
    public class JoinOnExpression : JoinConditionExpression
    {
        public JoinOnExpression(ColumnExpression leftColumn, string onOperator, ColumnExpression rightColumn)
            : base()
        {
            this.LeftColumn = leftColumn;
            this.RightColumn = rightColumn;
            this.OnOperator = onOperator;
        }

        #region 属性

        /// <summary>
        /// 左表匹配键
        /// </summary>
        public ColumnExpression LeftColumn { get; set; }

        public string OnOperator { get; set; }
        /// <summary>
        /// 右表匹配键
        /// </summary>
        public ColumnExpression RightColumn { get; set; }
 
        #endregion
    }


}