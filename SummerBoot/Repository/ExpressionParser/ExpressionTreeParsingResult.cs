using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser;

/// <summary>
/// Expression tree parsing results
/// ���ʽ���������
/// </summary>
public class ExpressionTreeParsingResult
{
    public string Sql { get; set; }

    public string CountSql { get; set; }
    public DynamicParameters Parameters { get; set; }
}