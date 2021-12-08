using System.Collections.Generic;

namespace DatabaseParser.ExpressionParser
{
    /// <summary>
    /// 解析器配置值
    /// </summary>
    public class ParserOption
    {
        public int TrueMapNum { get; set; } = 1;
        public int FalseMapNum { get; set; } = 0;
    }
}