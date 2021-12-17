using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlParse
{
    public class SqlParser
    {
        public SqlParser(string parameterPrefix, string leftQuote, string rightQuote)
        {
            this.parameterPrefix = parameterPrefix;
            this.leftQuote = leftQuote;
            this.rightQuote = rightQuote;
        }

        private string parameterPrefix;
        private string leftQuote;
        private string rightQuote;
        /// <summary>
        /// 符号列表
        /// </summary>
        public List<string> SymbolList { get; set; } = new List<string>() { " ", ",", ".", "@", ":", "=", ">", "<", "*", "`", "%", "\\", "(", ")", "[", "]", "-", "+" };
        /// <summary>
        /// 通用关键字列表
        /// </summary>
        public List<string> BaseKeyWordList { get; set; } = new List<string>() { "select", "from", "where", "by", "order", "group", "update", "delete", "insert", "as", "distinct", "sum", "max", "min", "avg" };
        /// <summary>
        /// 关键字列表
        /// </summary>
        /// <returns></returns>
        public virtual List<string> KeyWordList() => BaseKeyWordList;

        private string singleQuotes = "'";

        public List<SqlToken> Parse(string sql)
        {
            var tokenList = ParseToken(sql);
            tokenList = AnalysisToken(tokenList);
            return tokenList;
        }

        /// <summary>
        /// 初步拆分token
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected List<SqlToken> ParseToken(string sql)
        {
            //多个空格替换成一个
            Regex replaceSpace = new Regex(@"\s{1,}", RegexOptions.IgnoreCase);
            sql = replaceSpace.Replace(sql, " ");

            var result = new List<SqlToken>();

            var reader = new StringReader(sql);
            //当前指针
            var i = 0;
            //文本的开始位置
            var startPosition = 0;
            var text = "";
            //上一个字符是否为符号
            var previewTokenIsSymbol = false;
            //所有符号
            var allCharacter = new List<string>();
            var hasSingleQuote = false;

            while (true)
            {
                var content = reader.Read();

                var newChar = (char)content;
                //如果是分隔符或者如果上一个类型为符号类型，则添加
                var newCharString = newChar.ToString();

                //当前是否是最后一个，或者当前字符为符号
                var isFinalCharacterOrCurrentCharacterIsSymbol = SymbolList.Contains(newCharString) || content == -1;
                if (isFinalCharacterOrCurrentCharacterIsSymbol || previewTokenIsSymbol)
                {

                    var sqlTokenType = GetTokenType(text);
                    var token = new SqlToken(sqlTokenType, text, startPosition, i - 1);
                    startPosition = i;
                    result.Add(token);
                    if (isFinalCharacterOrCurrentCharacterIsSymbol)
                    {
                        previewTokenIsSymbol = true;
                    }
                    else if (previewTokenIsSymbol)
                    {
                        previewTokenIsSymbol = false;
                    }

                    text = "";
                }


                text += newChar;

                allCharacter.Add(newCharString);

                //Console.WriteLine("值为" + newChar);
                i++;
                //到最后一个则退出
                if (content == -1)
                {
                    break;
                }

                if (newCharString == singleQuotes)
                {
                    hasSingleQuote = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 分析token
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        protected List<SqlToken> AnalysisToken(List<SqlToken> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                SqlToken nextToken = null;
                SqlToken previewToken = null;
                if (i + 1 < tokens.Count)
                {
                    nextToken = tokens[i + 1];
                }

                if (i > 0)
                {
                    previewToken = tokens[i - 1];
                }

                //如果是参数笔记符号，则后面一定是参数类型
                if (token.Text == parameterPrefix)
                {
                    if (nextToken != null)
                    {
                        nextToken.TokenType = SqlTokenType.Variable;
                    }
                }

                //如果左右都是字段或者表明的标记符号，则中间的一定是表名或者字段名
                if (previewToken != null && nextToken != null && previewToken.Text == leftQuote &&
                    nextToken.Text == rightQuote)
                {
                    token.TokenType = SqlTokenType.Identifier;
                }
            }
            return tokens;
        }
        /// <summary>
        /// 判断token类型
        /// </summary>
        /// <returns></returns>
        private SqlTokenType GetTokenType(string text)
        {
            if (SymbolList.Contains(text))
            {
                return SqlTokenType.Symbol;
            }

            if (KeyWordList().Contains(text.ToLower()))
            {
                return SqlTokenType.KeyWord;
            }

            //var firstLetter = text.Substring(0, 1);
            //if (firstLetter == parameterPrefix)
            //{
            //    return SqlTokenType.Variable;
            //}

            //if (base.SpecialCharactersList.Contains(text))
            //{
            //    return SqlTokenType.SpecialCharacters;
            //}

            //if (base.OperatorList.Contains(text))
            //{
            //    return SqlTokenType.Operator;
            //}

            //if (base.FunctionList.Contains(firstLetter))
            //{
            //    return SqlTokenType.Function;
            //}

            //if (base.KeyWordList.Contains(firstLetter))
            //{
            //    return SqlTokenType.Function;
            //}

            return SqlTokenType.Identifier;
        }
    }
}
