using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>, IDbExecuteAndQuery, IAsyncQueryable<T>
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        List<SelectItem<T>> SelectItems { set; get; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        MultiQueryContext<T> MultiQueryContext { set; get; }
        int ExecuteUpdate();

        /// <summary>
        /// ���ɾ�����ҳ�Ľ��
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage();

        /// <summary>
        /// ���ɾ�����ҳ�Ľ��
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage(IPageable pageable);

        #region sync
        /// <summary>
        /// Get entity by id
        /// ͨ��id���ʵ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get(object id);
        /// <summary>
        /// Get all entities
        /// ��ȡ����ʵ��
        /// </summary>
        /// <returns></returns>
        List<T> GetAll();
        /// <summary>
        /// Update Entity
        /// ����ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        int Update(T t);
        /// <summary>
        /// Update entity list
        /// ����ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        void Update(List<T> list);
        /// <summary>
        /// Delete Entity
        /// ɾ��ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        int Delete(T t);
        /// <summary>
        /// Delete Entity list
        /// ɾ��ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        void Delete(List<T> list);
        /// <summary>
        /// Deleting entities by condition
        /// ͨ������ɾ��ʵ��
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int Delete(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Insert Entity
        /// ����ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        T Insert(T t);
        /// <summary>
        /// Insert entity list
        /// ����ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        List<T> Insert(List<T> list);
        /// <summary>
        /// Fast bulk insert
        /// ������������
        /// </summary>
        /// <param name="list"></param>
        void FastBatchInsert(List<T> list);

        #endregion sync

        #region async
        /// <summary>
        /// Get entity by id
        /// ͨ��id���ʵ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync(object id);
        /// <summary>
        /// Get all entities
        /// ��ȡ����ʵ��
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetAllAsync();
        /// <summary>
        /// Update Entity
        /// ����ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(T t);
        /// <summary>
        /// Update entity list
        /// ����ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        Task UpdateAsync(List<T> list);
        /// <summary>
        /// Delete Entity
        /// ɾ��ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(T t);
        /// <summary>
        /// Deleting entities by condition
        /// ͨ������ɾ��ʵ��
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Delete Entity list
        /// ɾ��ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task DeleteAsync(List<T> list);
        /// <summary>
        /// Insert Entity
        /// ����ʵ��
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<T> InsertAsync(T t);
        /// <summary>
        /// Insert entity list
        /// ����ʵ���б�
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<List<T>> InsertAsync(List<T> list);
        /// <summary>
        /// Fast bulk insert
        /// ������������
        /// </summary>
        /// <param name="list"></param>
        Task FastBatchInsertAsync(List<T> list);

        #endregion async

    }
}