using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestProject1
{

    //Use it as `optionsBuilder.ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>();`
    //`optionsBuilder` is the same var used to call `UseSqlServer()`
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class SqlServer2008QueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
    {
        private readonly QueryTranslationPostprocessorDependencies _dependencies;
        private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;
        public SqlServer2008QueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
            => new SqlServer2008QueryTranslationPostprocessor(
                _dependencies,
                _relationalDependencies,
                queryCompilationContext);

        public class SqlServer2008QueryTranslationPostprocessor : SqlServerQueryTranslationPostprocessor
        {
            public SqlServer2008QueryTranslationPostprocessor(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies, QueryCompilationContext queryCompilationContext)
                : base(dependencies, relationalDependencies, queryCompilationContext)
            {
            }

            public override Expression Process(Expression query)
            {
                query = base.Process(query);
                query = new Offset2RowNumberConvertVisitor(query, SqlExpressionFactory).Visit(query);
                return query;
            }

            private class Offset2RowNumberConvertVisitor : ExpressionVisitor
            {
                private static readonly Func<SelectExpression, SqlExpression, string, ColumnExpression> GenerateOuterColumnAccessor;

                static Offset2RowNumberConvertVisitor()
                {
                    var method = typeof(SelectExpression).GetMethod("GenerateOuterColumn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(SqlExpression), typeof(string) }, null);
                    if (method?.ReturnType != typeof(ColumnExpression))
                        throw new InvalidOperationException("SelectExpression.GenerateOuterColumn() is not found");
                    GenerateOuterColumnAccessor = (Func<SelectExpression, SqlExpression, string, ColumnExpression>)method.CreateDelegate(typeof(Func<SelectExpression, SqlExpression, string, ColumnExpression>));
                }

                private readonly Expression root;
                private readonly ISqlExpressionFactory sqlExpressionFactory;

                public Offset2RowNumberConvertVisitor(Expression root, ISqlExpressionFactory sqlExpressionFactory)
                {
                    this.root = root;
                    this.sqlExpressionFactory = sqlExpressionFactory;
                }

                protected override Expression VisitExtension(Expression node)
                {
                    if (node is SelectExpression se)
                        node = VisitSelect(se);
                    return base.VisitExtension(node);
                }

                private Expression VisitSelect(SelectExpression selectExpression)
                {
                    var oldOffset = selectExpression.Offset;
                    if (oldOffset == null)
                        return selectExpression;

                    var oldLimit = selectExpression.Limit;
                    var oldOrderings = selectExpression.Orderings;
                    //order by in subQuery without TOP N is invalid.
                    var newOrderings = oldOrderings.Count > 0 && (oldLimit != null || selectExpression == root)
                        ? oldOrderings.ToList()
                        : new List<OrderingExpression>();
                    selectExpression = selectExpression.Update(selectExpression.Projection.ToList(),
                                                               selectExpression.Tables.ToList(),
                                                               selectExpression.Predicate,
                                                               selectExpression.GroupBy.ToList(),
                                                               selectExpression.Having,
                                                               orderings: newOrderings,
                                                               limit: null,
                                                               offset: null,
                                                               selectExpression.IsDistinct,
                                                               selectExpression.Alias);
                    var rowOrderings = oldOrderings.Count != 0 ? oldOrderings
                        : new[] { new OrderingExpression(new SqlFragmentExpression("(SELECT 1)"), true) };
                    _ = selectExpression.PushdownIntoSubquery();
                    var subQuery = (SelectExpression)selectExpression.Tables[0];
                    var projection = new RowNumberExpression(Array.Empty<SqlExpression>(), rowOrderings, oldOffset.TypeMapping);
                    var left = GenerateOuterColumnAccessor(subQuery, projection, "row");
                    selectExpression.ApplyPredicate(sqlExpressionFactory.GreaterThan(left, oldOffset));
                    if (oldLimit != null)
                    {
                        if (oldOrderings.Count == 0)
                        {
                            selectExpression.ApplyPredicate(sqlExpressionFactory.LessThanOrEqual(left, sqlExpressionFactory.Add(oldOffset, oldLimit)));
                        }
                        else
                        {
                            //the above one not working when used as subQuery with orderBy
                            selectExpression.ApplyLimit(oldLimit);
                        }
                    }
                    return selectExpression;
                }
            }
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}