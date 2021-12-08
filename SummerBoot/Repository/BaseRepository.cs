using System;
using Dapper.Contrib.Extensions;
using SummerBoot.Core;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Threading.Tasks;
using DatabaseParser.Base;
using DatabaseParser.ExpressionParser;

namespace SummerBoot.Repository
{
    public class BaseRepository<T> : Repository<T> where T : class
    {

        public BaseRepository(IUnitOfWork Uow, IDbFactory dbFactory):base(DatabaseType.Mysql)
        {
            this.uow = Uow;
            this.dbFactory = dbFactory;
        }

        private IUnitOfWork uow;
        private IDbFactory dbFactory;
        protected IDbConnection dbConnection;
        protected IDbTransaction dbTransaction;
        private int cmdTimeOut = 1200;

        protected void OpenDb()
        {
            dbConnection = uow.ActiveNumber == 0 ? dbFactory.GetDbConnection() : dbFactory.GetDbTransaction().Connection;
            dbTransaction = uow.ActiveNumber == 0 ? null : dbFactory.GetDbTransaction();
        }

        protected void CloseDb()
        {
            if (uow.ActiveNumber == 0)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

  
    }
}