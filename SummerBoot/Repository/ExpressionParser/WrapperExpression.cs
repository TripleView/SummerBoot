using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser;

internal class WrapperExpression : Expression
{
    /// <summary>
    /// Is the processing completed
    /// 是否处理完毕
    /// </summary>
    public bool IsHandled { get; set; }
    public SqlExpression SqlExpression { get; set; }

    public SqlExpression CountSqlExpression { get; set; }

    public List<SqlExpression> SqlExpressions { get; set; }
    /// <summary>
    /// Property Type
    /// 属性类型
    /// </summary>
    public Type PropertyType { get; set; }
    /// <summary>
    /// Parameters
    /// 参数
    /// </summary>
    public DynamicParameters Parameters { get; set; }
    /// <summary>
    /// 分页参数
    /// pagingParams
    /// </summary>
    public Pageable Pageable { get; set; }
    /// <summary>
    /// 内部分页参数
    /// internal pagingParams
    /// </summary>
    public InternalPageable InternalPageable { get; set; }

    public bool? IsIgnoreWhenUpdate { get; set; }
}