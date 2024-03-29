﻿using System;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class SelectItem<T>
    {
       public Expression<Func<T,object>> Select { set; get; }
       public object Value { get; set; }
    }
}