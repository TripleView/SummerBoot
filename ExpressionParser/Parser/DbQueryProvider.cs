using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpressionParser.Base;
using ExpressionParser.Parser.Dialect;

namespace ExpressionParser.Parser
{
    public delegate TR ExecuteFunc<out TR>(DbQueryResult parameter);

    public class DbQueryProvider<T> : IQueryProvider
    {
        public QueryFormatter queryFormatter;

        private Repository<T> linkRepository;
        public DbQueryProvider(DatabaseType databaseType, Repository<T> linkRepository)
        {
            this.linkRepository = linkRepository;

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
                case DatabaseType.Sqlite:
                    this.queryFormatter = new SqliteQueryFormatter();
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

        public List<TBaseType> ExecuteList<TBaseType>(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();

            return linkRepository.QueryList<TBaseType>(param);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();

            return linkRepository.Query<TResult>(param);

        }

        public DbQueryResult GetDbQueryDetail()
        {
            return queryFormatter.GetDbQueryDetail();
        }
    }
}