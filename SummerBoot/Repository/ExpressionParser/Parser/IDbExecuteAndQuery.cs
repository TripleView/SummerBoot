using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IDbExecuteAndQuery
    {
        #region sync
        /// <summary>
        /// Get list data through sql statement
        /// 通过sql语句获取列表数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<TResult> QueryList<TResult>(string sql, object param = null);
        /// <summary>
        /// Get a single entity through sql statement
        /// 通过sql语句获取单个实体
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        TResult QueryFirstOrDefault<TResult>(string sql, object param = null);
        /// <summary>
        /// Execute SQL statements
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        int Execute(string sql, object param = null);
        /// <summary>
        /// Paging query through sql statement
        /// 通过sql语句分页查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="pageParameter"></param>
        /// <returns></returns>
        Page<TResult> QueryPage<TResult>(string sql, Pageable pageParameter, object param = null);
        #endregion

        #region async
        /// <summary>
        /// Get list data through sql statement
        /// 通过sql语句获取列表数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null);
        /// <summary>
        /// Get a single entity through sql statement
        /// 通过sql语句获取单个实体
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null);
        /// <summary>
        /// Execute SQL statements
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object param = null);
        /// <summary>
        /// Paging query through sql statement
        /// 通过sql语句分页查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="pageParameter"></param>
        /// <returns></returns>
        Task<Page<TResult>> QueryPageAsync<TResult>(string sql, Pageable pageParameter, object param = null);
        #endregion
    }
}