using System;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.MultiQuery
{

    public class SelectMultiQueryBody2<T1, T2, TResult>
    {
        public JoinBody2<T1, T2> Source { get; set; }
        public Expression<Func<JoinCondition<T1, T2>, TResult>> Select { get; set; }
    }

    public class SelectMultiQueryBody2<T1, T2, TResult, TAutoFill> : SelectMultiQueryBody2<T1, T2, TResult>
    {
        /// <summary>
        /// 自动填充的部分
        /// </summary>
        public Expression<Func<JoinCondition<T1, T2>, TAutoFill>> AuToFill { get; set; }
    }

    public class SelectMultiQueryBody3<T1, T2, T3, TResult>
    {
        public JoinBody3<T1, T2, T3> Source { get; set; }
        public Expression<Func<JoinCondition<T1, T2, T3>, TResult>> Select { get; set; }
    }

    public class SelectMultiQueryBody3<T1, T2, T3, TResult, TAutoFill> : SelectMultiQueryBody3<T1, T2, T3, TResult>
    {
        /// <summary>
        /// 自动填充的部分
        /// </summary>
        public Expression<Func<JoinCondition<T1, T2, T3>, TAutoFill>> AuToFill { get; set; }
    }

    public class SelectMultiQueryBody4<T1, T2, T3,T4, TResult>
    {
        public JoinBody4<T1, T2, T3,T4> Source { get; set; }
        public Expression<Func<JoinCondition<T1, T2, T3,T4>, TResult>> Select { get; set; }
    }

    public class SelectMultiQueryBody4<T1, T2, T3,T4, TResult, TAutoFill> : SelectMultiQueryBody4<T1, T2, T3,T4, TResult>
    {
        /// <summary>
        /// 自动填充的部分
        /// </summary>
        public Expression<Func<JoinCondition<T1, T2, T3,T4>, TAutoFill>> AuToFill { get; set; }
    }
}

