using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.ExpressionParser;

public class WrapperExpression : Expression
{
    /// <summary>
    /// Is the processing completed
    /// 是否处理完毕
    /// </summary>
    public bool IsHandled { get; set; }
    public SqlExpression SqlExpression { get; set; }

    public List<SqlSelectItemExpression> SqlSelectItemExpressions { get; set; }
    /// <summary>
    /// Property Type
    /// 属性类型
    /// </summary>
    public Type PropertyType { get; set; }
}