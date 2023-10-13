using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// 多个列表达式
    /// </summary>
    public class ColumnsExpression : DbBaseExpression
    {
        public ColumnsExpression(List<ColumnExpression> columnExpressions,Type type=null) : base((ExpressionType)DbExpressionType.Columns,type )
        {
            ColumnExpressions = columnExpressions;
        }
        public List<ColumnExpression> ColumnExpressions { get; set; }
    }
}