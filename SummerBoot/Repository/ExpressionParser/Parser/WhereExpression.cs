using System;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// where表达式
    /// </summary>
    public class WhereExpression : Expression
    {
        public WhereExpression(string @operator)
        {
            this.NodeType = (ExpressionType)DbExpressionType.Where;
            this.Operator = @operator;
        }

        public override ExpressionType NodeType { get; }
        public override Type Type { get; }
        /// <summary>
        /// 操作符，比如=,>等
        /// </summary>
        public string Operator { get; set; }

        public string NodeTypeName => ((DbExpressionType)NodeType).ToString();
        /// <summary>
        /// 左边的部分
        /// </summary>
        public WhereExpression Left { get; set; }
        /// <summary>
        /// 右边的部分
        /// </summary>
        public WhereExpression Right { get; set; }

        /// <summary>
        /// 值的类型
        /// </summary>
        public Type ValueType { get; set; }
    }
}