using SummerBoot.Repository.SqlParser.Dialect;
using System.Linq;
using Xunit;

namespace SqlParser.Test
{
    public class SqlServerTest1 : SqlServerParser
    {
        [Fact]
        public void TestSqlServerPage1()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a order by a.name";
            var parser = new SqlServerParser();
            var result= parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by fullname) pageNo, sbInner.* FROM (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by a.name) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by a.name) sbCount", result.CountSql);
            Assert.Equal(2,result.SqlParameters.Count);
            Assert.Equal(10,result.SqlParameters.FirstOrDefault(it=>it.ParameterName== "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestSqlServerPage2()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a order by a.city";
            var parser = new SqlServerParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by address) pageNo, sbInner.* FROM (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by a.city) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by a.city) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestSqlServerPage3()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a order by id";
            var parser = new SqlServerParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by id) pageNo, sbInner.* FROM (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by id) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a order by id) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestSqlServerPage4WithNotOrderBy()
        {
            var sql = "select id,a.name as fullname,a.city address from person2 a";
            var parser = new SqlServerParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by id ) pageNo, sbInner.* FROM (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  id,a.name as fullname,a.city address from person2 a) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestSqlServerPage5WithNotOrderBy()
        {
            var sql = "select a.name as fullname,id,a.city address from person2 a";
            var parser = new SqlServerParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by fullname ) pageNo, sbInner.* FROM (select TOP 100 PERCENT  a.name as fullname,id,a.city address from person2 a) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  a.name as fullname,id,a.city address from person2 a) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestSqlServerPage6WithNotOrderBy()
        {
            var sql = "select a.city address,id,a.name as fullname from person2 a";
            var parser = new SqlServerParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by address ) pageNo, sbInner.* FROM (select TOP 100 PERCENT  a.city address,id,a.name as fullname from person2 a) sbInner) sbOuter where pageNo > @sbPageSkip and pageNo <= @sbPageSkip + @sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select TOP 100 PERCENT  a.city address,id,a.name as fullname from person2 a) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }
    }
}
