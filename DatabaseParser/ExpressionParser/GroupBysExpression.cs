using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using DatabaseParser.Util;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// 多个group by表达式
    /// </summary>
    public class GroupBysExpression : Expression
    {
        public GroupBysExpression(List<GroupByExpression> groupByExpressions)
        {
            GroupByExpressions = groupByExpressions;
        }
        public List<GroupByExpression> GroupByExpressions { get; set; }
    }
}