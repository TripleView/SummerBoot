using System;
using System.Linq;
using SqlParser.Dialect;
using Xunit;

namespace SqlParser.Test
{
    public class MySqlTest1 : MysqlParser
    {

        [Fact]
        public void TestMysqlPage1()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a order by a.name";
            var parser = new MysqlParser();
            var result= parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select id,a.name as fullname,a.city address from person2 a order by a.name) sbOuter limit @sbPageSkip,@sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select id,a.name as fullname,a.city address from person2 a order by a.name) sbCount", result.CountSql);
            Assert.Equal(2,result.SqlParameters.Count);
            Assert.Equal(10,result.SqlParameters.FirstOrDefault(it=>it.ParameterName== "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

       
    }
}
