using System;
using System.Linq.Expressions;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// Join 表达式
    /// </summary>
    public class JoinExpression : QueryExpression
    {
        public JoinExpression(Type type, QueryExpression left, QueryExpression right, Expression leftKey, Expression rightKey)
            : base((ExpressionType)DbExpressionType.Join, type)
        {
            Left = left;
            Right = right;
            LeftKey = leftKey;
            RightKey = rightKey;
        }

        #region 属性

        /// <summary>
        /// 左表
        /// </summary>
        public QueryExpression Left { get; set; }

        /// <summary>
        /// 右表
        /// </summary>
        public QueryExpression Right { get; set; }

        /// <summary>
        /// 左表匹配键
        /// </summary>
        public Expression LeftKey { get; set; }

        /// <summary>
        /// 右表匹配键
        /// </summary>
        public Expression RightKey { get; set; }

        /// <summary>
        /// 左别名
        /// </summary>
        public string LeftAlias { get { return Left.Alias; } }

        /// <summary>
        /// 右别名
        /// </summary>
        public string RightAlias { get { return Right.Alias; } }

        #endregion
    }
}