using Dapper.Contrib.Extensions;
using SummerBoot.Core;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {

        //public BaseRepository(IUnitOfWork Uow)
        //{
        //    this.Uow = Uow;
        //}
        [Autowired(true)]
        public IUnitOfWork Uow { set; get; }

        [Autowired(true)]
        public IDbFactory DbFactory { set; get; }

        public IList<T> GetAll()
        {
            var dbcon = DbFactory.ShortDbConnection;
            var result = dbcon.GetAll<T>().ToList();

            dbcon.Close();
            dbcon.Dispose();

            return result;
        }

        public T Get(object id)
        {
            var dbcon = DbFactory.ShortDbConnection;

            var result = dbcon.Get<T>(id);

            dbcon.Close();
            dbcon.Dispose();

            return result;
        }

        public T Insert(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Insert(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return t;
        }

        public IList<T> Insert(IList<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Insert(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return t;
        }

        public void Delete(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Delete(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

        }

        public void Delete(IList<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Delete(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

        }

        public void Update(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Update(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

        }

        public void Update(IList<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Update(t, DbFactory.LongDbTransaction);
            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            var dbcon = DbFactory.ShortDbConnection;
            var result = dbcon.GetAllAsync<T>();

            result.ContinueWith(it =>
            {
                dbcon.Close();
                dbcon.Dispose();
            });
       

            return result;
        }

        public Task<T> GetAsync(object id)
        {
            var dbcon = DbFactory.ShortDbConnection;

            var result = dbcon.GetAsync<T>(id);

            result.ContinueWith(it =>
            {
                dbcon.Close();
                dbcon.Dispose();
            });

            return result;
        }

        public Task<T> InsertAsync(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result= dbcon.InsertAsync(t, DbFactory.LongDbTransaction);

            result.ContinueWith(it =>
            {
                if (Uow.ActiveNumber == 0)
                {
                    dbcon.Close();
                    dbcon.Dispose();
                }
            });
         

            return Task.FromResult(t);
        }

        public Task<IEnumerable<T>> InsertAsync(IEnumerable<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = dbcon.InsertAsync(t.ToList(), DbFactory.LongDbTransaction);

            result.ContinueWith(it =>
            {
                if (Uow.ActiveNumber == 0)
                {
                    dbcon.Close();
                    dbcon.Dispose();
                }
            });


            return Task.FromResult(t);
        }

        public Task UpdateAsync(T t)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(IList<T> t)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(IList<T> t)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(T t)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<T>> InsertAsync(IList<T> t)
        {
            throw new System.NotImplementedException();
        }
    }
}