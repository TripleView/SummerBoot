using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public static class QueryableMethodsExtension
    {
        public static List<MethodInfo> QueryMethodInfos =
            typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

        public static Dictionary<Type, MethodInfo> AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>();
        public static Dictionary<Type, MethodInfo> SumWithSelectorMethods = new Dictionary<Type, MethodInfo>();

        static QueryableMethodsExtension()
        {
            var numericTypes = new[]
            {
                typeof(int),
                typeof(int?),
                typeof(long),
                typeof(long?),
                typeof(float),
                typeof(float?),
                typeof(double),
                typeof(double?),
                typeof(decimal),
                typeof(decimal?)
            };

            foreach (var numericType in numericTypes)
            {
                SumWithSelectorMethods[numericType] = InternalGetSumAvgWithSelector(nameof(Queryable.Sum), numericType);
                AverageWithSelectorMethods[numericType] = InternalGetSumAvgWithSelector(nameof(Queryable.Average), numericType);
            }
        }

        public static MethodInfo GetMethodInfoWithSelector(string methodName, bool isGenericMethod = true, int? genericTypeParameterCount = 2, int? parameterCount = null, Type type = null)
        {
            var d = QueryMethodInfos.Where(it => it.Name == nameof(Queryable.Count)).ToList();
            var c = d.Select(it => it.GetGenericArguments().Length).ToList();
            var b = d.Select(it => it.GetParameters().Length).ToList();
            var query = QueryCondition.True<MethodInfo>();
            query = query.And(it => it.Name == methodName);
            if (isGenericMethod)
            {
                query = query.And(it => it.IsGenericMethod);
            }
            if (genericTypeParameterCount.HasValue)
            {
                query = query.And(it => it.GetGenericArguments().Length == genericTypeParameterCount);
            }
            if (parameterCount.HasValue)
            {
                query = query.And(it => it.GetParameters().Length == parameterCount);
            }
            if (type != null)
            {
                query = query.And(it => it.GetParameters().Length > 1 && it.GetParameters()[1].ParameterType ==
                                                           typeof(Expression<>).MakeGenericType(
                                                               typeof(Func<,>).MakeGenericType(it.GetGenericArguments()[0],
                                                                   type)));
            }

            return QueryMethodInfos.First(query.Compile());
        }

        private static MethodInfo InternalGetSumAvgWithSelector(string methodName, Type type)
        {
            var result = QueryMethodInfos.First(it => it.Name == methodName
                                                      && it.IsGenericMethod && it.GetParameters().Length == 2
                                                      && it.GetParameters()[1].ParameterType ==
                                                      typeof(Expression<>).MakeGenericType(
                                                          typeof(Func<,>).MakeGenericType(it.GetGenericArguments()[0],
                                                              type)));
            return result;
        }

        public static MethodInfo GetSumAvgWithSelector(string methodName, Type type)
        {
            if (methodName == nameof(Queryable.Sum))
            {
                return SumWithSelectorMethods[type];
            }
            if (methodName == nameof(Queryable.Average))
            {
                return AverageWithSelectorMethods[type];
            }

            throw new NotSupportedException(methodName);
        }
    }
}
