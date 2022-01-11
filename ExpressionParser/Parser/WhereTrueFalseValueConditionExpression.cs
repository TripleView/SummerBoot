using System;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    public class WhereTrueFalseValueConditionExpression:WhereExpression
    {
        public WhereTrueFalseValueConditionExpression(bool value):base("")
        {
            this.NodeType = (ExpressionType)DbExpressionType.WhereTrueCondition;
            this.Value = value;
        }

        public bool Value { get; set; }
        public override ExpressionType NodeType { get; }
        public override Type Type { get; }
    }
}