using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SummerBoot.Repository;

internal static class RepositoryMethods
{
    public static ILambdaRepository<T> Where<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static ILambdaRepository<T> WhereIf<T>(ILambdaRepository<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IOrderLambdaRepository<T> OrderBy<T, TKey>(ILambdaRepository<T> source, Expression<Func<T, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IOrderLambdaRepository<T> OrderByDescending<T, TKey>(ILambdaRepository<T> source, Expression<Func<T, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IOrderLambdaRepository<T> ThenBy<T, TKey>(ILambdaRepository<T> source, Expression<Func<T, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IOrderLambdaRepository<T> ThenByDescending<T, TKey>(ILambdaRepository<T> source, Expression<Func<T, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }


    public static IGroupLambdaRepository<T, TKey> GroupBy<T, TKey>(ILambdaRepository<T> source, Expression<Func<T, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static ILambdaRepository<TResult> Select<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T, TKey, TResult>(IGroupLambdaRepository<T, TKey> source, Expression<Func<IGrouping<TKey, T>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static List<T> ToList<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IPageLambdaRepository<T> Take<T>(ILambdaRepository<T> source, int count)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IPageLambdaRepository<T> Skip<T>(ILambdaRepository<T> source, int count)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<int> CountAsync<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<int> CountAsync<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static T First<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<T> FirstAsync<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static T First<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<T> FirstAsync<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static T FirstOrDefault<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<T> FirstOrDefaultAsync<T>(ILambdaRepository<T> source)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static T FirstOrDefault<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<T> FirstOrDefaultAsync<T>(ILambdaRepository<T> source, Expression<Func<T, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    public static Page<T> ToPage<T>(ILambdaRepository<T> source, IPageable pageable)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    public static Task<Page<T>> ToPageAsync<T>(ILambdaRepository<T> source, IPageable pageable)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Max<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<TResult> MaxAsync<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<TResult> MinAsync<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<TResult> SumAsync<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static Task<TResult> AverageAsync<T, TResult>(ILambdaRepository<T> source, Expression<Func<T, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class RepositoryMethodsCache
{
    public static readonly MethodInfo Where =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo WhereIf =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.WhereIf) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo OrderBy =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo OrderByDescending =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo ThenBy =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo ThenByDescending =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo GroupBy =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Select =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo GroupBySelect =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo ToList =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.ToList) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo Take =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Take) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo Skip =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Skip) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo Count =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo CountWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo CountAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.CountAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo CountAsyncWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.CountAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo First =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.First) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo FirstWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.First) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo FirstAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo FirstAsyncWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo FirstOrDefault =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstOrDefault) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo FirstOrDefaultWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstOrDefault) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo FirstOrDefaultAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstOrDefaultAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);

    public static readonly MethodInfo FirstOrDefaultAsyncWithSelector =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.FirstOrDefaultAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);

    public static readonly MethodInfo ToPage =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.ToPage) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo ToPageAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.ToPageAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    public static readonly MethodInfo Max =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo MaxAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.MaxAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Min =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo MinAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.MinAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Sum =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo SumAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.SumAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Average =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo AverageAsync =
        typeof(RepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(RepositoryMethods.AverageAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);
}