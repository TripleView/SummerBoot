using System;
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

        public BaseRepository(IUnitOfWork Uow, IDbFactory dbFactory)
        {
            this.Uow = Uow;
            this.DbFactory = dbFactory;
        }

        private IUnitOfWork Uow { set; get; }

        private IDbFactory DbFactory { set; get; }

        public List<T> GetAll()
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

        public long BatchInsert(List<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = dbcon.Insert(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return result;
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

        public void BatchDelete(List<T> t)
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

        public void BatchUpdate(List<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            dbcon.Update(t, DbFactory.LongDbTransaction);
            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            var dbcon = DbFactory.ShortDbConnection;
            var result = await dbcon.GetAllAsync<T>();

            dbcon.Close();
            dbcon.Dispose();

            return result.ToList();
        }

        public async Task<T> GetAsync(object id)
        {
            var dbcon = DbFactory.ShortDbConnection;

            var result = await dbcon.GetAsync<T>(id);

            dbcon.Close();
            dbcon.Dispose();

            return result;
        }

        public async Task<T> InsertAsync(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.InsertAsync(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return t;
        }

        public async Task<long> BatchInsertAsync(List<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.InsertAsync(t.ToList(), DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return result;
        }

        public async Task UpdateAsync(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.UpdateAsync(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }
        }

        public async Task BatchUpdateAsync(List<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.UpdateAsync(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }
        }

        public async Task DeleteAsync(T t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.DeleteAsync(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }
        }

        public async Task BatchDeleteAsync(List<T> t)
        {
            var dbcon = Uow.ActiveNumber == 0 ? DbFactory.ShortDbConnection : DbFactory.LongDbConnection;
            var result = await dbcon.DeleteAsync(t, DbFactory.LongDbTransaction);

            if (Uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }
        }
    }
}