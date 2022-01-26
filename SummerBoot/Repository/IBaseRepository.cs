using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IBaseRepository<T> : ExpressionParser.Parser.IRepository<T> where T : class
    {
        #region sync
        T Get(dynamic id);

        List<T> GetAll();
        int Update(T t);
        void Update(List<T> list);
        void Delete(T t);
        void Delete(List<T> list);

        int Delete(Expression<Func<T, bool>> predicate);

        T Insert(T t);
        List<T> Insert(List<T> list);
        #endregion sync

        #region async
        Task<T> GetAsync(dynamic id);
        Task<List<T>> GetAllAsync();
        Task<int> UpdateAsync(T t);
        Task UpdateAsync(List<T> list);

        Task<int> DeleteAsync(T t);
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(List<T> list);

        Task<T> InsertAsync(T t);
        Task<List<T>> InsertAsync(List<T> list);
        #endregion async
    }
}