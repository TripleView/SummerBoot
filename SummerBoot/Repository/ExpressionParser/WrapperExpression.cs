using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.ExpressionParser;

public class WrapperExpression : Expression
{
    /// <summary>
    /// Is the processing completed
    /// �Ƿ������
    /// </summary>
    public bool IsHandled { get; set; }
    public SqlExpression SqlExpression { get; set; }

    public List<SqlSelectItemExpression> SqlSelectItemExpressions { get; set; }
    /// <summary>
    /// Property Type
    /// ��������
    /// </summary>
    public Type PropertyType { get; set; }
}