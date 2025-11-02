//using SummerBoot.Core;
//using SummerBoot.Repository.ExpressionParser.Parser;
//using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace SummerBoot.Repository
//{

//    public static partial class RepositoryExtension
//    {
//        #region Left Join

//        public static JoinBody2<T1, T2> LeftJoin<T1, T2>(this IQueryable<T1> source, T2 t2, Expression<Func<JoinCondition<T1, T2>, bool>> condition) where T1 : class where T2 : class
//        {

//            if (source == null) throw new ArgumentNullException("source");

//            if (!(source is IRepository<T1> repository))
//            {
//                throw new Exception("only support IRepository");
//            }

//            if (repository == null) throw new ArgumentNullException("source");

//            var result = new JoinBody2<T1, T2>()
//            {
//                JoinType = JoinType.LeftJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T2),
//                JoinTableAlias = nameof(T2)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody3<T1, T2, T3> LeftJoin<T1, T2, T3>(this JoinBody2<T1, T2> source, T3 t3, Expression<Func<JoinCondition<T1, T2, T3>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody3<T1, T2, T3>()
//            {
//                JoinType = JoinType.LeftJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T3),
//                JoinTableAlias = nameof(T3)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody4<T1, T2, T3, T4> LeftJoin<T1, T2, T3, T4>(this JoinBody3<T1, T2, T3> source, T4 t4, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody4<T1, T2, T3, T4>()
//            {
//                JoinType = JoinType.LeftJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T4),
//                JoinTableAlias = nameof(T4)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }


//        #endregion

//        #region Right Join

//        public static JoinBody2<T1, T2> RightJoin<T1, T2>(this IQueryable<T1> source, T2 t2, Expression<Func<JoinCondition<T1, T2>, bool>> condition) where T1 : class where T2 : class
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            if (!(source is IRepository<T1> repository))
//            {
//                throw new Exception("only support IRepository");
//            }

//            if (repository == null) throw new ArgumentNullException("source");

//            var result = new JoinBody2<T1, T2>()
//            {
//                JoinType = JoinType.RightJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T2),
//                JoinTableAlias = nameof(T2)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody3<T1, T2, T3> RightJoin<T1, T2, T3>(this JoinBody2<T1, T2> source, T3 t3, Expression<Func<JoinCondition<T1, T2, T3>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody3<T1, T2, T3>()
//            {
//                JoinType = JoinType.RightJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T3),
//                JoinTableAlias = nameof(T3)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody4<T1, T2, T3, T4> RightJoin<T1, T2, T3, T4>(this JoinBody3<T1, T2, T3> source, T4 t4, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody4<T1, T2, T3, T4>()
//            {
//                JoinType = JoinType.RightJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T4),
//                JoinTableAlias = nameof(T4)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }


//        #endregion

//        #region Inner Join

//        public static JoinBody2<T1, T2> InnerJoin<T1, T2>(this IQueryable<T1> source, T2 t2, Expression<Func<JoinCondition<T1, T2>, bool>> condition) where T1 : class where T2 : class
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            if (!(source is IRepository<T1> repository))
//            {
//                throw new Exception("only support IRepository");
//            }

//            if (repository == null) throw new ArgumentNullException("source");

//            var result = new JoinBody2<T1, T2>()
//            {
//                JoinType = JoinType.InnerJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T2),
//                JoinTableAlias = nameof(T2)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody3<T1, T2, T3> InnerJoin<T1, T2, T3>(this JoinBody2<T1, T2> source, T3 t3, Expression<Func<JoinCondition<T1, T2, T3>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody3<T1, T2, T3>()
//            {
//                JoinType = JoinType.InnerJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T3),
//                JoinTableAlias = nameof(T3)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }

//        public static JoinBody4<T1, T2, T3, T4> InnerJoin<T1, T2, T3, T4>(this JoinBody3<T1, T2, T3> source, T4 t4, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> condition)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var repository = source.Repository;

//            var result = new JoinBody4<T1, T2, T3, T4>()
//            {
//                JoinType = JoinType.InnerJoin,
//                Repository = repository,
//                Condition = condition,
//                JoinTable = typeof(T4),
//                JoinTableAlias = nameof(T4)
//            };
//            repository.MultiQueryContext.JoinItems.Add(result);
//            return result;
//        }


//        #endregion

//        #region Select

//        public static SelectMultiQueryBody2<T1, T2, TResult> Select<T1, T2, TResult>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> select)
//        {

//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody2<T1, T2, TResult>()
//            {
//                Select = select,
//                Source = source
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            return result;
//        }

//        public static SelectMultiQueryBody3<T1, T2, T3, TResult> Select<T1, T2, T3, TResult>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> select)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody3<T1, T2, T3, TResult>()
//            {
//                Select = select,
//                Source = source
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            return result;
//        }

//        public static SelectMultiQueryBody4<T1, T2, T3, T4, TResult> Select<T1, T2, T3, T4, TResult>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> select)
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody4<T1, T2, T3, T4, TResult>()
//            {
//                Select = select,
//                Source = source
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            return result;
//        }

//        public static SelectMultiQueryBody2<T1, T2, TResult, TAutoFill> Select<T1, T2, TResult, TAutoFill>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> select, Expression<Func<JoinCondition<T1, T2>, TAutoFill>> autoFill) where TAutoFill : class, new()
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody2<T1, T2, TResult, TAutoFill>()
//            {
//                Select = select,
//                Source = source,
//                AuToFill = autoFill
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            source.Repository.MultiQueryContext.MultiQuerySelectAutoFillItem = autoFill;
//            return result;
//        }

//        public static SelectMultiQueryBody3<T1, T2, T3, TResult, TAutoFill> Select<T1, T2, T3, TResult, TAutoFill>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> select, Expression<Func<JoinCondition<T1, T2, T3>, TAutoFill>> autoFill) where TAutoFill : class, new()
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody3<T1, T2, T3, TResult, TAutoFill>()
//            {
//                Select = select,
//                Source = source,
//                AuToFill = autoFill
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            source.Repository.MultiQueryContext.MultiQuerySelectAutoFillItem = autoFill;
//            return result;
//        }

//        public static SelectMultiQueryBody4<T1, T2, T3, T4, TResult, TAutoFill> Select<T1, T2, T3, T4, TResult, TAutoFill>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> select, Expression<Func<JoinCondition<T1, T2, T3, T4>, TAutoFill>> autoFill) where TAutoFill : class, new()
//        {
//            if (source == null) throw new ArgumentNullException("source");

//            var result = new SelectMultiQueryBody4<T1, T2, T3, T4, TResult, TAutoFill>()
//            {
//                Select = select,
//                Source = source,
//                AuToFill = autoFill
//            };
//            source.Repository.MultiQueryContext.MultiQuerySelectItem = select;
//            source.Repository.MultiQueryContext.MultiQuerySelectAutoFillItem = autoFill;
//            return result;
//        }


//        #endregion

//        private static void ClearRepository<T>(IRepository<T> repository)
//        {
//            repository.MultiQueryContext = new MultiQueryContext<T>();
//            repository.SelectItems = new List<SelectItem<T>>();
//        }

//        #region ToList

//        public static List<TResult> ToList<T1, T2, TResult, TAutoFill>(this SelectMultiQueryBody2<T1, T2, TResult, TAutoFill> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static List<TResult> ToList<T1, T2, T3, TResult, TAutoFill>(this SelectMultiQueryBody3<T1, T2, T3, TResult, TAutoFill> selectMultiQueryBody)
//        {
//            if (selectMultiQueryBody.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static List<TResult> ToList<T1, T2, T3, T4, TResult, TAutoFill>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult, TAutoFill> selectMultiQueryBody)
//        {
//            if (selectMultiQueryBody.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }


//        public static List<TResult> ToList<T1, T2, TResult>(this SelectMultiQueryBody2<T1, T2, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static List<TResult> ToList<T1, T2, T3, TResult>(this SelectMultiQueryBody3<T1, T2, T3, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static List<TResult> ToList<T1, T2, T3, T4, TResult>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = repository.QueryList<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        //async
//        public static async Task<List<TResult>> ToListAsync<T1, T2, TResult, TAutoFill>(this SelectMultiQueryBody2<T1, T2, TResult, TAutoFill> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static async Task<List<TResult>> ToListAsync<T1, T2, T3, TResult, TAutoFill>(this SelectMultiQueryBody3<T1, T2, T3, TResult, TAutoFill> selectMultiQueryBody)
//        {
//            if (selectMultiQueryBody.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static async Task<List<TResult>> ToListAsync<T1, T2, T3, T4, TResult, TAutoFill>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult, TAutoFill> selectMultiQueryBody)
//        {
//            if (selectMultiQueryBody.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }


//        public static async Task<List<TResult>> ToListAsync<T1, T2, TResult>(this SelectMultiQueryBody2<T1, T2, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static async Task<List<TResult>> ToListAsync<T1, T2, T3, TResult>(this SelectMultiQueryBody3<T1, T2, T3, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }

//        public static async Task<List<TResult>> ToListAsync<T1, T2, T3, T4, TResult>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult> selectMultiQueryBody2)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository);

//                var result = await repository.QueryListAsync<TResult>(parameter.Sql,parameter.GetDynamicParameters());
//                ClearRepository(repository);
//                return result;
//            }
//            return new List<TResult>();
//        }
//        #endregion

//        #region Where
//        public static JoinBody2<T1, T2> Where<T1, T2>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.And(on);
//            }
//            return source;
//        }

//        public static JoinBody2<T1, T2> OrWhere<T1, T2>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.Or(on);
//            }
//            return source;
//        }
//        public static JoinBody2<T1, T2> WhereIf<T1, T2>(this JoinBody2<T1, T2> source, bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.Where(on);
//            }
//            else
//            {
//                return source;
//            }
//        }

//        public static JoinBody2<T1, T2> OrWhereIf<T1, T2>(this JoinBody2<T1, T2> source, bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.OrWhere(on);
//            }
//            else
//            {
//                return source;
//            }
//        }

//        public static JoinBody3<T1, T2, T3> Where<T1, T2, T3>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2, T3>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.And(on);
//            }
//            return source;
//        }

//        public static JoinBody3<T1, T2, T3> OrWhere<T1, T2, T3>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2, T3>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.Or(on);
//            }
//            return source;
//        }
//        public static JoinBody3<T1, T2, T3> WhereIf<T1, T2, T3>(this JoinBody3<T1, T2, T3> source, bool condition, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.Where(on);
//            }
//            else
//            {
//                return source;
//            }
//        }

//        public static JoinBody3<T1, T2, T3> OrWhereIf<T1, T2, T3>(this JoinBody3<T1, T2, T3> source, bool condition, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.OrWhere(on);
//            }
//            else
//            {
//                return source;
//            }
//        }

//        public static JoinBody4<T1, T2, T3, T4> Where<T1, T2, T3, T4>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.And(on);
//            }
//            return source;
//        }

//        public static JoinBody4<T1, T2, T3, T4> OrWhere<T1, T2, T3, T4>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> on)
//        {
//            if (source.Repository.MultiQueryContext.MultiQueryWhereItem == null)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = on;
//            }
//            else if (source.Repository.MultiQueryContext.MultiQueryWhereItem is Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> previewOn)
//            {
//                source.Repository.MultiQueryContext.MultiQueryWhereItem = previewOn.Or(on);
//            }
//            return source;
//        }
//        public static JoinBody4<T1, T2, T3, T4> WhereIf<T1, T2, T3, T4>(this JoinBody4<T1, T2, T3, T4> source, bool condition, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.Where(on);
//            }
//            else
//            {
//                return source;
//            }
//        }

//        public static JoinBody4<T1, T2, T3, T4> OrWhereIf<T1, T2, T3, T4>(this JoinBody4<T1, T2, T3, T4> source, bool condition, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> on)
//        {
//            if (condition)
//            {
//                return source.OrWhere(on);
//            }
//            else
//            {
//                return source;
//            }
//        }
//        #endregion

//        #region Order By

//        public static JoinBody2<T1, T2> OrderBy<T1, T2, TResult>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody3<T1, T2, T3> OrderBy<T1, T2, T3, TResult>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody4<T1, T2, T3, T4> OrderBy<T1, T2, T3, T4, TResult>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        #endregion

//        #region Then BY


//        public static JoinBody2<T1, T2> ThenBy<T1, T2, TResult>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody3<T1, T2, T3> ThenBy<T1, T2, T3, TResult>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody4<T1, T2, T3, T4> ThenBy<T1, T2, T3, T4, TResult>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Asc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }
//        #endregion

//        #region ThenByDescending

//        public static JoinBody2<T1, T2> ThenByDescending<T1, T2, TResult>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody3<T1, T2, T3> ThenByDescending<T1, T2, T3, TResult>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody4<T1, T2, T3, T4> ThenByDescending<T1, T2, T3, T4, TResult>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> orderBy)
//        {
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = false
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }
//        #endregion

//        #region OrderByDescending

//        public static JoinBody2<T1, T2> OrderByDescending<T1, T2, TResult>(this JoinBody2<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody3<T1, T2, T3> OrderByDescending<T1, T2, T3, TResult>(this JoinBody3<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        public static JoinBody4<T1, T2, T3, T4> OrderByDescending<T1, T2, T3, T4, TResult>(this JoinBody4<T1, T2, T3, T4> source, Expression<Func<JoinCondition<T1, T2, T3, T4>, TResult>> orderBy)
//        {
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Clear();
//            var orderByItem = new MultiQueryOrderByItem()
//            {
//                OrderByType = OrderByType.Desc,
//                OrderByExpression = orderBy,
//                IsMain = true
//            };
//            source.Repository.MultiQueryContext.MultiQueryOrderByItems.Add(orderByItem);
//            return source;
//        }

//        #endregion

//        #region ToPage

//        public static Page<TResult> ToPage<T1, T2, TResult>(this SelectMultiQueryBody2<T1, T2, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = dbQueryProvider.linkRepository.QueryPage<TResult>(parameter.Sql,null,null);;
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        public static Page<TResult> ToPage<T1, T2, T3, TResult>(this SelectMultiQueryBody3<T1, T2, T3, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = dbQueryProvider.linkRepository.QueryPage<TResult>(parameter.Sql,null,null);;
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        public static Page<TResult> ToPage<T1, T2, T3, T4, TResult>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = dbQueryProvider.linkRepository.QueryPage<TResult>(parameter.Sql,null,null);
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        public static async Task<Page<TResult>> ToPageAsync<T1, T2, TResult>(this SelectMultiQueryBody2<T1, T2, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = await dbQueryProvider.linkRepository.QueryPageAsync<TResult>(parameter.Sql,null,null);
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        public static async Task<Page<TResult>> ToPageAsync<T1, T2, T3, TResult>(this SelectMultiQueryBody3<T1, T2, T3, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = await dbQueryProvider.linkRepository.QueryPageAsync<TResult>(parameter.Sql, null, null);
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        public static async Task<Page<TResult>> ToPageAsync<T1, T2, T3, T4, TResult>(this SelectMultiQueryBody4<T1, T2, T3, T4, TResult> selectMultiQueryBody2, IPageable pageable)
//        {
//            if (selectMultiQueryBody2.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
//            {
//                if (!(selectMultiQueryBody2.Source.Repository is IRepository<T1> repository))
//                {
//                    throw new Exception("only support IRepository");
//                }

//                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody2.Source.Repository, pageable);

//                var result = await dbQueryProvider.linkRepository.QueryPageAsync<TResult>(parameter.Sql,null,null);;
//                ClearRepository(repository);
//                return result;
//            }
//            return new Page<TResult>();
//        }

//        #endregion
//    }
//}

