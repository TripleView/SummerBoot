using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// orderBy表达式
    /// </summary>
    public class OrderByExpression : Expression
    {
        public OrderByExpression(OrderByType orderByType,ColumnExpression columnExpression)
        {
            this.NodeType = (ExpressionType)DbExpressionType.OrderBy;
            this.ColumnExpression = columnExpression;
            this.OrderByType = orderByType;
        }

        /// <summary>
        /// 排序的字段
        /// </summary>
        public ColumnExpression ColumnExpression { get; set; }

        public override ExpressionType NodeType { get; }
        public override Type Type { get; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public OrderByType OrderByType { get; set; }

        public string NodeTypeName => ((DbExpressionType)NodeType).ToString();
    }

    public enum OrderByType
    {
        Asc,
        Desc
    }
}