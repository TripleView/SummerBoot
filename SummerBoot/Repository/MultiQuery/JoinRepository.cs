using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Repository.MultiQuery;

public class JoinRepository<T1> : IJoinRepository<T1>
{
    public ILambdaRepository<T1> Source { get; }

    public JoinRepository(ILambdaRepository<T1> source)
    {
        Source = source;
    }
    public IJoinRepository<T1, T2> LeftJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepository2MethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2> RightJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepository2MethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2> InnerJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepository2MethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    private IJoinRepository<T1, T2> InternalJoin<T2>(
        ILambdaRepository<T2> joinTable,
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
            Expression.Quote(on));

        var body = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinRepository<T1, T2>(body);

        return result;
    }
}