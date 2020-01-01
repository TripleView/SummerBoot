using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IRepository<T> where T : class
    {
        IList<T> GetAll();
        T Get(object id);

        T Insert(T t);
        IList<T> Insert(IList<T> t);

        void Update(T t);
        void Update(IList<T> t);

        void Delete(IList<T> t);
        void Delete(T t);

        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(object id);

        Task<T> InsertAsync(T t);
        Task<IEnumerable<T>> InsertAsync(IList<T> t);

        Task UpdateAsync(T t);
        Task UpdateAsync(IList<T> t);

        Task DeleteAsync(IList<T> t);
        Task DeleteAsync(T t);
    }
}