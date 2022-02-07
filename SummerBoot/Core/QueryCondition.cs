using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace SummerBoot.Core
{
    /// <summary>
    /// lambda扩展类
    /// </summary>
    public static class QueryCondition
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        /// <summary>
        /// 如果可空值不为空则进行and操作，否则返回原表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="first"></param>
        /// <param name="obj"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> AndIfNullableHasValue<T, T2>(this Expression<Func<T, bool>> first, T2? obj, Expression<Func<T, bool>> second) where T2 : struct
        {
            if (obj.HasValue)
            {
                return first.And(second);
            }

            return first;
        }

        /// <summary>
        /// 如果可空值不为空则进行or操作，否则返回原表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="first"></param>
        /// <param name="obj"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> OrIfNullableHasValue<T,T2>(this Expression<Func<T, bool>> first, T2? obj, Expression<Func<T, bool>> second) where T2 : struct
        {
            if (obj.HasValue)
            {
               return first.Or(second);
            }

            return first;
        }

        /// <summary>
        /// 如果字符串不为空则进行and操作，否则返回原表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="str"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> AndIfStringIsNotEmpty<T>(this Expression<Func<T, bool>> first, string str, Expression<Func<T, bool>> second)
        {
            if (str.HasText())
            {
                return first.And(second);
            }

            return first;
        }

        /// <summary>
        /// 如果字符串不为空则进行or操作，否则返回原表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="str"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> OrIfStringIsNotEmpty<T>(this Expression<Func<T, bool>> first, string str, Expression<Func<T, bool>> second)
        {
            if (str.HasText())
            {
                return first.Or(second);
            }

            return first;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            ParameterExpression p = first.Parameters[0];

            SubstExpressionVisitor visitor = new SubstExpressionVisitor();
            visitor.subst[second.Parameters[0]] = p;

            Expression body = Expression.OrElse(first.Body, visitor.Visit(second.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            ParameterExpression p = first.Parameters[0];

            SubstExpressionVisitor visitor = new SubstExpressionVisitor();
            visitor.subst[second.Parameters[0]] = p;

            Expression body = Expression.AndAlso(first.Body, visitor.Visit(second.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        internal class SubstExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            public Dictionary<Expression, Expression> subst = new Dictionary<Expression, Expression>();

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Expression newValue;
                if (subst.TryGetValue(node, out newValue))
                {
                    return newValue;
                }
                return node;
            }
        }
    }
}
