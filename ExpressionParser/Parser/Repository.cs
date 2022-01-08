﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpressionParser.Base;
using ExpressionParser.Parser.Dialect;

namespace ExpressionParser.Parser
{
    public class Repository<T> : IRepository<T>
    {
        public Repository(DatabaseType databaseType)
        {
            Provider = new DbQueryProvider(databaseType);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        public Repository()
        {

        }

        public void Init(DatabaseType databaseType)
        {
            Provider = new DbQueryProvider(databaseType);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        //public object Test(ExecuteFunc<T> t)
        //{

        //}

        public virtual Task<int> ExecuteAsync(DbQueryResult param)
        {
            return default;
        }

        public virtual int Execute(DbQueryResult param)
        {
            return default;
        }

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
        public List<SelectItem<T>> SelectItems { get; set; } = new List<SelectItem<T>>();

        public IEnumerator<T> GetEnumerator()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbQueryResultByExpression(Expression);
                var result=QueryList<T>(dbParam);
                
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
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.GetDbQueryDetail();
            }

            return null;
        }

        public DbQueryResult InternalInsert(T insertEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Insert(insertEntity);;
            }

            return null;
        }

        public DbQueryResult InternalUpdate(T updateEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Update(updateEntity); ;
            }
            return null;
        }

        public DbQueryResult InternalDelete(T deleteEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Delete(deleteEntity);
            }
            return null;
        }

        public DbQueryResult InternalDelete(Expression predicate)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.DeleteByExpression<T>(predicate);
            }
            return null;
        }
        
        public DbQueryResult InternalGet(dynamic id)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
               return dbQueryProvider.queryFormatter.Get<T>(id);
            }
            return null;
        }

        public DbQueryResult InternalGetAll()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
              return dbQueryProvider.queryFormatter.GetAll<T>();
            }
            return null;
        }

        public void ExecuteUpdate()
        {
            if (this.SelectItems?.Count == 0)
            {
                throw new Exception("set value first");
            }

            if (Provider is DbQueryProvider dbQueryProvider)
            {
               var dbQueryResult=  dbQueryProvider.queryFormatter.ExecuteUpdate(Expression,this.SelectItems);
               this.Execute(dbQueryResult);
            }
        }

        public async Task ExecuteUpdateAsync()
        {
            if (this.SelectItems?.Count == 0)
            {
                throw new Exception("set value first");
            }

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbQueryResult = dbQueryProvider.queryFormatter.ExecuteUpdate(Expression, this.SelectItems);
                await this.ExecuteAsync(dbQueryResult);
            }
        }
    }

}