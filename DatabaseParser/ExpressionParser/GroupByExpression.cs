﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// orderBy表达式
    /// </summary>
    public class GroupByExpression : Expression
    {
        public GroupByExpression(ColumnExpression columnExpression)
        {
            this.NodeType = (ExpressionType)DbExpressionType.GroupBy;
            this.ColumnExpression = columnExpression;
        }

        /// <summary>
        /// 排序的字段
        /// </summary>
        public ColumnExpression ColumnExpression { get; set; }

        public override ExpressionType NodeType { get; }
        public override Type Type { get; }
    

        public string NodeTypeName => ((DbExpressionType)NodeType).ToString();
    }

}