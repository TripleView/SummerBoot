using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser;

/// <summary>
/// Expression tree parsing results
/// 表达式树解析结果
/// </summary>
public class ExpressionTreeParsingResult
{
    public string Sql { get; set; }
    public DynamicParameters Parameters { get; set; }
}