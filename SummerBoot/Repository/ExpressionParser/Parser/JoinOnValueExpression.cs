using System;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Join on表达式
    /// </summary>
    public class JoinOnValueExpression : JoinConditionExpression
    {
        public JoinOnValueExpression(ColumnExpression column, string onOperator, object value)
            : base()
        {
            this.Column = column;
            this.Value = value;
            this.OnOperator = onOperator;
        }

        #region 属性

        /// <summary>
        /// 左表匹配键
        /// </summary>
        public ColumnExpression Column { get; set; }

        public string OnOperator { get; set; }
        /// <summary>
        /// 右表匹配键
        /// </summary>
        public object Value { get; set; }

        #endregion

        public override string NodeTypeName => nameof(JoinOnValueExpression);
    }


}