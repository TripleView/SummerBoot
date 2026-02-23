using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.MultiQuery;

public interface IJoinQueryable<T1>
{
    IJoinQueryable<T1, T2> LeftJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinQueryable<T1, T2> RightJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinQueryable<T1, T2> InnerJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
    //IEnumerable<TResult> Select<TResult>(Func<(T1 T1, T2 T2), TResult> selector);
}

public interface IJoinQueryable<T1, T2>
{
    IJoinQueryable<T1, T2, T3> LeftJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinQueryable<T1, T2, T3> RightJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinQueryable<T1, T2, T3> InnerJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinOrderQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
    //IEnumerable<TResult> Select<TResult>(Func<(T1 T1, T2 T2), TResult> selector);
}

public interface IJoinOrderQueryable<T1, T2>
{
    IJoinOrderQueryable<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
}

public class JoinOrderQueryable<T1, T2>
public interface IJoinQueryable<T1, T2, T3>
{
    IQueryable<JoinCondition<T1, T2, T3>> Source { get; }
    //IJoinQueryable<T1, T2> LeftJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> RightJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> InnerJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
    //IEnumerable<TResult> Select<TResult>(Func<(T1 T1, T2 T2), TResult> selector);
}

public class JoinQueryable<T1, T2, T3> : IJoinQueryable<T1, T2, T3>
{
   public IQueryable<JoinCondition<T1, T2, T3>> Source { get; }

   //public IQueryable<JoinCondition<T1, T2, T3>> Source { get; }

    public JoinQueryable(IQueryable<JoinCondition<T1, T2, T3>> source)
   {
       Source = source;
   }
}


public class JoinQueryable<T1, T2> : IJoinQueryable<T1, T2>
{
    public IQueryable<T1> Source { get; }
    public IQueryable<JoinCondition<T1, T2>> Body { get; }

    public JoinQueryable(IQueryable<T1> source, IQueryable<JoinCondition<T1, T2>> body)
    {
        Source = source;
        Body = body;
    }

    public IJoinOrderQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
    }

    public IJoinQueryable<T1, T2, T3> LeftJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2, T3> RightJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2, T3> InnerJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    private IJoinOrderQueryable<T1, T2> InternalOrderBy<TKey>(
        Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(keySelector)
        );

        var r = Source.Provider.CreateQuery<JoinCondition<T1, T2>>(callExpr);
        var result=new 
        return result;
    }


    private IJoinQueryable<T1, T2, T3> InternalJoin<T3>(
        IQueryable<T3> joinTable,
        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
        if (on == null) throw new ArgumentNullException(nameof(on));

        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            joinTable.Expression,
            Expression.Quote(on)
        );
        var r2 = Source.Provider.CreateQuery<JoinCondition<T1, T2, T3>>(callExpr);
        var r = Source.Provider.CreateQuery<JoinCondition<T1, T2, T3>>(callExpr);
        var result = new JoinQueryable<T1, T2, T3>(r);
        return result;
    }

}


public class JoinQueryable<T1> : IJoinQueryable<T1>
{
    public IQueryable<T1> Source { get; }

    public JoinQueryable(IQueryable<T1> source)
    {
        Source = source;
    }
    public IJoinQueryable<T1, T2> LeftJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2> RightJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2> InnerJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    private IJoinQueryable<T1, T2> InternalJoin<T2>(
        IQueryable<T2> joinTable,
        Expression<Func<JoinCondition<T1, T2>, bool>> on,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
        if (on == null) throw new ArgumentNullException(nameof(on));

        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            joinTable.Expression,
            Expression.Quote(on)
        );

        var body = Source.Provider.CreateQuery<JoinCondition<T1, T2>>(callExpr);
        var result = new JoinQueryable<T1, T2>(this.Source, body);

        return result;
    }

}