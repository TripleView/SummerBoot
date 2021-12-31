using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpressionParser.Parser;

namespace SummerBoot.Repository
{
    public interface IBaseRepository<T> : IRepository<T> where T : class
    {
        #region sync
        T Get(dynamic id);

        List<T> GetAll();
        void Update(T t);
        void Update(List<T> list);

        void Delete(T t);
        void Delete(List<T> list);

        void Delete(Expression<Func<T, bool>> predicate);

        T Insert(T t);
        List<T> Insert(List<T> list);
        #endregion sync

        #region async
        Task<T> GetAsync(dynamic id);
        Task<List<T>> GetAllAsync();
        Task UpdateAsync(T t);
        Task UpdateAsync(List<T> list);

        Task DeleteAsync(T t);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(List<T> list);

        Task<T> InsertAsync(T t);
        Task<List<T>> InsertAsync(List<T> list);
        #endregion async
    }
}