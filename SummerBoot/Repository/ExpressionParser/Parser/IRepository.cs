using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>, IDbExecuteAndQuery, IAsyncQueryable<T>
    {
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //List<SelectItem<T>> SelectItems { set; get; }
        [EditorBrowsable(EditorBrowsableState.Never)]

        int ExecuteUpdate();

        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage();

        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage(IPageable pageable);

        #region sync
        /// <summary>
        /// Get entity by id
        /// 通过id获得实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get(object id);
        /// <summary>
        /// Get all entities
        /// 获取所有实体
        /// </summary>
        /// <returns></returns>
        List<T> GetAll();
        /// <summary>
        /// Update Entity
        /// 更新实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        int Update(T t);
        /// <summary>
        /// Update entity list
        /// 更新实体列表
        /// </summary>
        /// <param name="list"></param>
        void Update(List<T> list);
        /// <summary>
        /// Delete Entity
        /// 删除实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        int Delete(T t);
        /// <summary>
        /// Delete Entity list
        /// 删除实体列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        void Delete(List<T> list);
        /// <summary>
        /// Deleting entities by condition
        /// 通过条件删除实体
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int Delete(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Insert Entity
        /// 插入实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        T Insert(T t);
        /// <summary>
        /// Insert entity list
        /// 插入实体列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        List<T> Insert(List<T> list);
        /// <summary>
        /// Fast bulk insert
        /// 快速批量插入
        /// </summary>
        /// <param name="list"></param>
        void FastBatchInsert(List<T> list);

        #endregion sync

        #region async
        /// <summary>
        /// Get entity by id
        /// 通过id获得实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync(object id);
        /// <summary>
        /// Get all entities
        /// 获取所有实体
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetAllAsync();
        /// <summary>
        /// Update Entity
        /// 更新实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(T t);
        /// <summary>
        /// Update entity list
        /// 更新实体列表
        /// </summary>
        /// <param name="list"></param>
        Task UpdateAsync(List<T> list);
        /// <summary>
        /// Delete Entity
        /// 删除实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(T t);
        /// <summary>
        /// Deleting entities by condition
        /// 通过条件删除实体
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Delete Entity list
        /// 删除实体列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task DeleteAsync(List<T> list);
        /// <summary>
        /// Insert Entity
        /// 插入实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<T> InsertAsync(T t);
        /// <summary>
        /// Insert entity list
        /// 插入实体列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<List<T>> InsertAsync(List<T> list);
        /// <summary>
        /// Fast bulk insert
        /// 快速批量插入
        /// </summary>
        /// <param name="list"></param>
        Task FastBatchInsertAsync(List<T> list);

        #endregion async

    }
}