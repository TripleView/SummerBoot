using System.Collections.Generic;

namespace SqlParse.Dialect
{
    public class MysqlParser:SqlParser
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