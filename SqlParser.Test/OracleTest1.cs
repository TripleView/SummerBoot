using System;
using System.Linq;
using SqlParser.Dialect;
using Xunit;

namespace SqlParser.Test
{
    public class OracleTest1: SqliteParser
    {
        [Fact]
        public void TestOrderByClause()
        {
            var sql = "select * from person a order by a.name";
            var orderByClause= this.GetOrderByClause(sql);
            Assert.Equal( "order by a.name", orderByClause);
        }

        [Fact]
        public void TestSelectFieldsClause()
        {
            var sql = "select id,a.name as fullname,a.city address from person a order by a.name";
            var selectFieldsClause = this.GetSelectFieldsClause(sql);
            Assert.Equal( "id,a.name as fullname,a.city address", selectFieldsClause);
        }

        [Fact]
        public void TestOraclePage1()
        {
            var sql = "select id,a.name as fullname,a.city address from person a order by a.name";
            var parser = new OracleParser();
            var result= parser.ParserPage(sql, 1, 10);
            Assert.Equal( "select * from (select row_number() over (order by fullname) pageNo, sbInner.* FROM (select id,a.name as fullname,a.city address from person a order by a.name) sbInner) sbOuter where pageNo > :sbPageSize and pageNo <= :sbPageSkip + :sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select id,a.name as fullname,a.city address from person a order by a.name) sbCount", result.CountSql);
            Assert.Equal(2,result.SqlParameters.Count);
            Assert.Equal(10,result.SqlParameters.FirstOrDefault(it=>it.ParameterName== "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestOraclePage2()
        {
            var sql = "select id,a.name as fullname,a.city address from person a order by a.city";
            var parser = new OracleParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by address) pageNo, sbInner.* FROM (select id,a.name as fullname,a.city address from person a order by a.city) sbInner) sbOuter where pageNo > :sbPageSize and pageNo <= :sbPageSkip + :sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select id,a.name as fullname,a.city address from person a order by a.city) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }

        [Fact]
        public void TestOraclePage3()
        {
            var sql = "select id,a.name as fullname,a.city address from person a order by id";
            var parser = new OracleParser();
            var result = parser.ParserPage(sql, 1, 10);
            Assert.Equal("select * from (select row_number() over (order by id) pageNo, sbInner.* FROM (select id,a.name as fullname,a.city address from person a order by id) sbInner) sbOuter where pageNo > :sbPageSize and pageNo <= :sbPageSkip + :sbPageSize", result.PageSql);
            Assert.Equal("select count(1) from (select id,a.name as fullname,a.city address from person a order by id) sbCount", result.CountSql);
            Assert.Equal(2, result.SqlParameters.Count);
            Assert.Equal(10, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSize")?.Value);
            Assert.Equal(0, result.SqlParameters.FirstOrDefault(it => it.ParameterName == "sbPageSkip")?.Value);
        }
    }
}
