using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.Core;

public class FunctionCallInfo
{
    public SqlExpression Body { get; set; }

    public List<SqlExpression> FunctionParameters { get; set; }

    public DynamicParameters DynamicParameters { get; set; }
    public Func<object,SqlVariableExpression> AddParameter { get; set; }
}