using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DatabaseParser.Base;
using DatabaseParser.ExpressionParser.Dialect;

namespace DatabaseParser.ExpressionParser
{
    public class DbQueryProvider : IQueryProvider
    {
        public QueryFormatter queryFormatter;
        public DbQueryProvider(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    this.queryFormatter = new SqlServerQueryFormatter();
                    break;
                case DatabaseType.Mysql:
                    this.queryFormatter = new MysqlQueryFormatter();
                    break;
                case DatabaseType.Oracle:
                    this.queryFormatter = new OracleQueryFormatter();
                    break;
            }
            
        }
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Repository<TElement>(expression,this);
        }

        public object Execute(Expression expression)
        {
           
            return null;
        }

        
        public TResult Execute<TResult>(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            return default;
        }

        public DbQueryResult GetDbQueryDetail()
        {
            return queryFormatter.GetDbQueryDetail();
        }
    }
}