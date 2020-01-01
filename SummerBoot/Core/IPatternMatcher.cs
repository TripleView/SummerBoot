namespace SummerBoot.Core
{
    /// <summary>
    /// 数据源是否符合某个验证规则
    /// </summary>
    public interface IPatternMatcher
    {
        bool Matches(string pattern, string source);
    }
}