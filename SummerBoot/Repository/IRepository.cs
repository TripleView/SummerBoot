using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T Get(object id);

        T Insert(T t);

        long BatchInsert(List<T> t);

        void Update(T t);
        void BatchUpdate(List<T> t);

        void Delete(T t);
        void BatchDelete(List<T> t);

        Task<List<T>> GetAllAsync();
        Task<T> GetAsync(object id);

        Task<T> InsertAsync(T t);
        Task<long> BatchInsertAsync(List<T> t);

        Task UpdateAsync(T t);
        Task BatchUpdateAsync(List<T> t);

        Task DeleteAsync(T t);
        Task BatchDeleteAsync(List<T> t);
    }
}