using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpressionParser.Base;

namespace ExpressionParser.Parser
{
    public class Repository<T> : IRepository<T>
    {
        public Repository(DatabaseType databaseType)
        {
            Provider = new DbQueryProvider<T>(databaseType,this);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        public Repository()
        {

        }

        public void Init(DatabaseType databaseType)
        {
            Provider = new DbQueryProvider<T>(databaseType,this);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        //public object Test(ExecuteFunc<T> t)
        //{

        //}

        public virtual TResult Query<TResult>(DbQueryResult param)
        {
            return default;
        }

        public virtual List<TResult> QueryList<TResult>(DbQueryResult param)
        {
            return default;
        }
        public Repository(Expression expression, IQueryProvider provider)
        {
            Provider = provider;
            Expression = expression;
        }


        public Type ElementType => typeof(T);

        public Expression Expression { get; private set; }


        public IQueryProvider Provider { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
                var result = dbQueryProvider.ExecuteList<T>(Expression);
                if (result == null)
                    yield break;
                foreach (var item in result)
                {
                    yield return item;
                }
            }
           
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        public DbQueryResult GetDbQueryDetail()
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
                return dbQueryProvider.GetDbQueryDetail();
            }

            return null;
        }

        public DbQueryResult InternalInsert(T insertEntity)
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Insert(insertEntity);;
            }

            return null;
        }

        public DbQueryResult InternalUpdate(T updateEntity)
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Update(updateEntity); ;
            }
            return null;
        }

        public DbQueryResult InternalDelete(T deleteEntity)
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Delete(deleteEntity);
            }
            return null;
        }

        public DbQueryResult InternalGet(dynamic id)
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Get<T>(id);
            }
            return null;
        }

        public DbQueryResult InternalGetAll()
        {
            if (Provider is DbQueryProvider<T> dbQueryProvider)
            {
              return dbQueryProvider.queryFormatter.GetAll<T>();
            }
            return null;
        }
    }

}