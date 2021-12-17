using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// 代表输出查询的表达式（Select、Table、Join等表达式）
    /// </summary>
    public abstract class QueryExpression : DbBaseExpression
    {
        protected QueryExpression(ExpressionType expressionType, Type type) : base(expressionType,type)
        {
            
        }

        /// <summary>
        /// 查询的别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 所有列表达式
        /// </summary>
        public virtual List<ColumnExpression> Columns { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public virtual Expression From { get; set; }
    }
}