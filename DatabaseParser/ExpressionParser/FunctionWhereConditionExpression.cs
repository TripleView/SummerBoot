﻿using System.Linq.Expressions;

namespace DatabaseParser.ExpressionParser
{
    public class FunctionWhereConditionExpression : WhereExpression
    {
        public FunctionWhereConditionExpression(string @operator, WhereExpression whereExpression) : base(@operator)
        {
            NodeType = (ExpressionType)DbExpressionType.FunctionWhereCondition;
            this.WhereExpression = whereExpression;
          
        }
        public override ExpressionType NodeType { get; }

        public WhereExpression WhereExpression { get; set; }

    }
}