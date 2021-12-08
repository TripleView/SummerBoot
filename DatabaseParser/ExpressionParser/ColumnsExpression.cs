using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using DatabaseParser.Util;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// 多个列表达式
    /// </summary>
    public class ColumnsExpression : Expression
    {
        public ColumnsExpression(List<ColumnExpression> columnExpressions)
        {
            ColumnExpressions = columnExpressions;
        }
        public List<ColumnExpression> ColumnExpressions { get; set; }
    }
}