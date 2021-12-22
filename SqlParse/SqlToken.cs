using System;
using System.Collections.Generic;
using System.Text;

namespace SqlParser
{
    /// <summary>
    /// 解析出来的词
    /// </summary>
    public class SqlToken
    {
        public SqlToken(SqlTokenType tokenType, string text, int startPosition, int endPosition)
        {
            TokenType = tokenType;
            Text = text;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        /// <summary>
        /// 词类型
        /// </summary>
        public SqlTokenType TokenType { get; set; }
        /// <summary>
        /// 具体文本
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// 开始位置
        /// </summary>
        public int StartPosition { get; }
        /// <summary>
        /// 结束位置
        /// </summary>
        public int EndPosition { get; }
        /// <summary>
        /// 文本长度
        /// </summary>
        public int Length => EndPosition - StartPosition + 1;

        public override string ToString()
        {
            return $"值为{Text},开始位置{StartPosition},结束位置{EndPosition},长度{Length},类型{TokenType.ToString()}";
        }
    }
}
