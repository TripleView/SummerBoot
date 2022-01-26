using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SummerBoot.Repository.SqlParser.Dto;

namespace SummerBoot.Repository.SqlParser
{
    public class SqlParser : ISqlParser
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
        /// 分页时，每页多少条
        /// </summary>
        protected string PageSizeName => $"sbPageSize";

        /// <summary>
        /// 分页时，跳过前面多少条
        /// </summary>
        protected string PageSkipName => $"sbPageSkip";

        /// <summary>
        /// 包装分页时，每页多少条
        /// </summary>
        protected string BoxPageSizeName => $"{parameterPrefix}{PageSizeName}";

        /// <summary>
        /// 包装分页时，跳过前面多少条
        /// </summary>
        protected string BoxPageSkipName => $"{parameterPrefix}{PageSkipName}";

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

        /// <summary>
        /// 获取order by语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string GetOrderByClause(string sql)
        {
            var words = sql.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            sql = string.Join(" ", words).ToLower();


            if (!sql.Contains("order by"))
            {
                return "";
            }
            else
            {
                var lastIndex = sql.LastIndexOf("order by");
                if (lastIndex >= 0)
                {
                    var orderByString = sql.Substring(lastIndex);
                    return orderByString;
                }
            }

            return "";
        }

        /// <summary>
        /// 获取select 出来的字段语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string GetSelectFieldsClause(string sql)
        {
            var words = sql.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            sql = string.Join(" ", words).ToLower();

            if (!sql.Contains("select") || !sql.Contains("from"))
            {
                return "";
            }
            else
            {
                var selectIndex = sql.IndexOf("select");
                var fromIndex = sql.IndexOf("from");
                var result = sql.Substring(selectIndex + 6, fromIndex - 6).Trim();
                return result;
            }

            return "";
        }

        public virtual ParserPageStringResult ParserPage(string sql, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取order by的字段
        /// </summary>
        /// <param name="sqlOrderBy"></param>
        /// <param name="newSqlOrderByRemovedArr"></param>
        /// <param name="orderByFields"></param>
        /// <exception cref="Exception"></exception>
        protected void GetOrderByFields(string sqlOrderBy, string[] newSqlOrderByRemovedArr, List<string> orderByFields)
        {
            sqlOrderBy = sqlOrderBy.Replace("order by", "").Trim();
            var field1 = sqlOrderBy.IndexOf(".") > -1 ? sqlOrderBy.Split('.')[1] : sqlOrderBy;
            var table2 = sqlOrderBy.IndexOf(".") > -1 ? sqlOrderBy.Split('.')[0] : "";
            var sort = "";
            var field2 = "";
            //判断是否是 字段 asc这种形式
            if (field1.IndexOf(" ") > -1)
            {
                var field2Arr = field1.Split(' ');
                if (field2Arr[1] == "asc")
                {
                    sort = "asc";
                }
                if (field2Arr[1] == "desc")
                {
                    sort = "desc";
                }

                field2 = field2Arr[0];
            }
            else
            {
                field2 = field1;
            }

            var tableField2 = string.IsNullOrWhiteSpace(table2) ? field2 : table2 + "." + field2;
            //判断是否有别名
            var hasAlias = false;
            var fieldAlias = "";

            foreach (var s1 in newSqlOrderByRemovedArr)
            {
                var s1Temp = s1.Trim();
                //类似 t1.a as b，包含空格才行。
                if (s1Temp.IndexOf(' ') > -1)
                {
                    var s1Arr = s1Temp.Split(' ');
                    var conditionNum = s1Temp.IndexOf("as") > -1 ? 2 : 1;
                    var field3 = s1Arr[0].Trim();
                    //获得字段名
                    var field4 = field3.IndexOf(".") > -1 ? field3.Split('.')[1] : field3;
                    var table4 = field3.IndexOf(".") > -1 ? field3.Split('.')[0] : "";
                    var tableField4 = string.IsNullOrWhiteSpace(table4) ? field4 : table4 + "." + field4;

                    if (field2 == field4 && tableField2 != tableField4) throw new Exception($"order by字段{tableField2}与select中的字段{tableField4}不一致，请确认");

                    if (tableField2 == tableField4 && !string.IsNullOrWhiteSpace(s1Arr[conditionNum].Trim()))
                    {
                        hasAlias = true;
                        fieldAlias = s1Temp.IndexOf("as") > -1 ? s1Arr[2].Trim() : s1Arr[1];
                        break;
                    }
                }
            }

            if (hasAlias)
            {
                if (!string.IsNullOrWhiteSpace(sort))
                {
                    fieldAlias += " " + sort;
                }
                orderByFields.Add(fieldAlias);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(sort))
                {
                    field2 += " " + sort;
                }
                orderByFields.Add(field2);
            }
        }
    }
}
