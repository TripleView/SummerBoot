using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Join 表达式
    /// </summary>
    public class JoinExpression : DbBaseExpression
    {
        public JoinExpression( JoinType joinType, TableExpression joinTable, string joinTableAlias)
            : base((ExpressionType)DbExpressionType.Join, typeof(object))
        {
            this.JoinTable = joinTable;
            this.JoinTableAlias = joinTableAlias;
            this.JoinType = joinType;
        }

        #region 属性
        /// <summary>
        /// 关联条件
        /// </summary>
        public JoinConditionExpression JoinCondition { get; set; }
       
        /// <summary>
        /// join方式
        /// </summary>
        public JoinType JoinType { get; set; }
        /// <summary>
        /// join的表
        /// </summary>
        public TableExpression JoinTable { get; set; }

        /// <summary>
        /// join表的别名
        /// </summary>
        public string JoinTableAlias { get; set; }
        #endregion
    }


}