using SummerBoot.Repository.SqlParser.Dialect;
using Xunit;

namespace SqlParser.Test
{
    public class SqlParserTest1 : SqliteParser
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
    }
}
