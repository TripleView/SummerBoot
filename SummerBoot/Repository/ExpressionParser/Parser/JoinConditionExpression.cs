using System;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Join on表达式
    /// </summary>
    public class JoinConditionExpression : DbBaseExpression
    {
        public JoinConditionExpression()
            : base((ExpressionType)DbExpressionType.JoinCondition, typeof(bool))
        {
        
        }

        /// <summary>
        /// 连接的左半部分
        /// </summary>
        public JoinConditionExpression Left { get; set; }
        /// <summary>
        /// 连接的右半部分
        /// </summary>
        public JoinConditionExpression Right { get; set; }
        /// <summary>
        /// 操作符
        /// </summary>
        public string Operator { get; set; }
    }


}