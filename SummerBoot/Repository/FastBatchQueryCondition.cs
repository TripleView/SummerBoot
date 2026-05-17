using System;
using System.Collections.Generic;
using SummerBoot.Repository.ExpressionParser.Parser;

namespace SummerBoot.Repository;

public class FastBatchQueryCondition
{
    public string Sql { get; set; }

    public List<FastBatchSqlParameter> FastBatchSqlParameters { get; set; }

    /// <summary>
    /// 列字段的信息
    /// </summary>
    public List<DbQueryResultPropertyInfoMapping> PropertyInfoMappings { get; set; }
}

public class FastBatchSqlParameter
{
    public string ParameterName { get; set; }
    public object Value { get; set; }
    /// <summary>
    /// 参数的类型
    /// </summary>
    public Type ParameterType { get; set; }

    public System.Data.DbType DbType { get; set; }
}