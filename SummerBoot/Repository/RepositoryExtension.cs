using Microsoft.AspNetCore.Mvc;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.MultiQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public static partial class RepositoryExtension
    {
        public static IJoinRepository<T1, T2> LeftJoin<T1, T2>(
            this IBaseRepository<T1> source,
            IBaseRepository<T2> joinTable,
            Expression<Func<JoinCondition<T1, T2>, bool>> on)
            where T1 : class where T2 : class
        {
            return new JoinRepository<T1>(source).LeftJoin(joinTable,on);
        }

        public static IJoinRepository<T1, T2> RightJoin<T1, T2>(
            this IBaseRepository<T1> source,
            IBaseRepository<T2> joinTable,
            Expression<Func<JoinCondition<T1, T2>, bool>> on)
            where T1 : class where T2 : class
        {
            return new JoinRepository<T1>(source).RightJoin(joinTable, on);
        }

        public static IJoinRepository<T1, T2> InnerJoin<T1, T2>(
            this IBaseRepository<T1> source,
            IBaseRepository<T2> joinTable,
            Expression<Func<JoinCondition<T1, T2>, bool>> on)
            where T1 : class where T2 : class
        {
            return new JoinRepository<T1>(source).InnerJoin(joinTable, on);
        }
    }
}