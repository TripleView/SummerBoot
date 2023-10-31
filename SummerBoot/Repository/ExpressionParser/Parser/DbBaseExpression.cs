using System;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// db基础表达式
    /// </summary>
    public class DbBaseExpression : Expression
    {
        protected DbBaseExpression(ExpressionType expressionType, Type type) : base()
        {
            Type = type;
            NodeType = expressionType;
        }
        public override ExpressionType NodeType { get; }
        public virtual string NodeTypeName => ((DbExpressionType)NodeType).ToString();
        public override Type Type { get; }
        
    }
}