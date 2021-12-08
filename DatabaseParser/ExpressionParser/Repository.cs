using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DatabaseParser.Base;

namespace DatabaseParser.ExpressionParser
{
    public class Repository<T> : IRepository<T>
    {

        public Repository(DatabaseType databaseType)
        {
            Provider = new DbQueryProvider(databaseType);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        public Repository(Expression expression, IQueryProvider provider)
        {
            Provider = provider;
            Expression = expression;
        }


        public Type ElementType => typeof(T);

        public Expression Expression { get; }


        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            var result = Provider.Execute<List<T>>(Expression);
            if (result == null)
                yield break;
            foreach (var item in result)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        public DbQueryResult GetDbQueryDetail()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.GetDbQueryDetail();
            }

            return null;
        }

        public void Insert(T insertEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                dbQueryProvider.queryFormatter.Insert(insertEntity);;
            }
        }

        public void Update(T updateEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                dbQueryProvider.queryFormatter.Update(updateEntity); ;
            }
        }

        public void Delete(T deleteEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                dbQueryProvider.queryFormatter.Delete(deleteEntity);
            }
        }

       
    }

}