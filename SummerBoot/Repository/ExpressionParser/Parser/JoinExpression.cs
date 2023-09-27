using System;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Join 表达式
    /// </summary>
    public class JoinExpression : DbBaseExpression
    {
        public JoinExpression(Type type, TableExpression leftTable, TableExpression rightTable, ColumnExpression leftColumn, string onOperator, ColumnExpression rightColumn)
            : base((ExpressionType)DbExpressionType.Join, type)
        {
            this.LeftTable = leftTable;
            this.RightTable = rightTable;
            this.LeftColumn = leftColumn;
            this.RightColumn = rightColumn;
            this.OnOperator = onOperator;
        }

        #region 属性

        /// <summary>
        /// 左表
        /// </summary>
        public TableExpression LeftTable { get; set; }

        /// <summary>
        /// 右表
        /// </summary>
        public TableExpression RightTable { get; set; }

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