using System.Linq.Expressions;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// where表达式里的条件表达式，比如a=2,c>3等等
    /// </summary>
    public class WhereConditionExpression : WhereExpression
    {
        public WhereConditionExpression(ColumnExpression columnExpression, string @operator, object value):base(@operator)
        {
            ColumnExpression = columnExpression;
            Value = value;
            NodeType= (ExpressionType)DbExpressionType.WhereCondition;
        }

        public override ExpressionType NodeType { get; }
        /// <summary>
        /// 列
        /// </summary>
        public ColumnExpression ColumnExpression { get; set; }
      
        /// <summary>
        /// 具体的值
        /// </summary>
        public object Value { get; set; }
        
    }
}