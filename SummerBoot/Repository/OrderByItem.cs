using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository;
/// <summary>
/// Sorting rules
/// 排序规则
/// </summary>
public class OrderByItem
{
    public string Field { get; set; }
    public SqlOrderByType OrderByType { get; set; } 
}