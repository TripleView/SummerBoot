using System.Linq;
using SummerBoot.Repository.SqlParser.Dialect;
using Xunit;

namespace SqlParser.Test
{
    public class SqliteTest1 : SqliteParser
    {

        [Fact]
        public void TestSqlitePage1()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a order by a.name";
            var parser = new SqliteParser();
            var result= parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select id,a.name as fullname,a.city address from person2 a order by a.name) sbOuter limit :sbPageSize offset :sbPageSkip", result.PageSql);
            Assert.Equal("select count(1) from (select id,a.name as fullname,a.city address from person2 a order by a.name) sbCount", result.CountSql);
            Assert.Equal(2,result.SqlParameters.Count);
            Assert.Equal(10,result.SqlParameters.FirstOrDefault(it=>it.ParameterName== "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

       
    }
}
