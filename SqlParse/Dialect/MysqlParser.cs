using System.Collections.Generic;
using SqlParser.Dto;

namespace SqlParser.Dialect
{
    public class MysqlParser : SqlParser
    {
        public MysqlParser() : base("@", "`", "`")
        {

        }

        public override List<string> KeyWordList()
        {
            BaseKeyWordList.Add("limit");

            return BaseKeyWordList;
        }

    }
}