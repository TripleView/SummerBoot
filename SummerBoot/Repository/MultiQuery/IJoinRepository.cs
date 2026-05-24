using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SummerBoot.Repository.MultiQuery;

public interface IJoinRepository<T1>
{
    IJoinRepository<T1, T2> LeftJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinRepository<T1, T2> RightJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinRepository<T1, T2> InnerJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
}


