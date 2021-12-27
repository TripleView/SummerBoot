using System;
using Dapper.Contrib.Extensions;
using SummerBoot.Core;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Dapper;
using System.Threading.Tasks;
using ExpressionParser.Base;
using ExpressionParser.Parser;

namespace SummerBoot.Repository
{
    public class BaseRepository<T> : Repository<T>, IBaseRepository<T> where T : class
    {

        public BaseRepository(IUnitOfWork uow, IDbFactory dbFactory,RepositoryOption repositoryOption) 
        {
            this.uow = uow;
            this.dbFactory = dbFactory;

            databaseType = DatabaseType.Mysql;

            if (repositoryOption.IsMysql)
            {
                databaseType = DatabaseType.Mysql;
            }

            if (repositoryOption.IsOracle)
            {
                databaseType = DatabaseType.Oracle;
            }


            if (repositoryOption.IsSqlServer)
            {
                databaseType = DatabaseType.SqlServer;
            }

            if (repositoryOption.IsSqlite)
            {
                databaseType = DatabaseType.Sqlite;
            }

            base.Init(databaseType);
        }

        private IUnitOfWork uow;
        private IDbFactory dbFactory;
        protected IDbConnection dbConnection;
        protected IDbTransaction dbTransaction;
        private DatabaseType databaseType;
        private int cmdTimeOut = 1200;


        public override List<TResult> QueryList<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.Query<TResult>(param.Sql, dynamicParameters, dbTransaction).ToList();

            CloseDb();
            return result;
        }

        public override TResult Query<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.QueryFirst<TResult>(param.Sql, dynamicParameters, dbTransaction);

            CloseDb();
            return result;
            //return result.FirstOrDefault();
        }

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

        public void Update(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Update(item);
            }
            CloseDb();
            this.uow.Commit();
        }

        public void Delete(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Delete(item);
            }
            CloseDb();
            this.uow.Commit();
        }

        public T Insert(T t)
        {
            var internalResult = InternalInsert(t);
         
            OpenDb();
            
            if (databaseType == DatabaseType.Oracle)
            {

            }
            else if (databaseType == DatabaseType.Sqlite)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);
                    dynamicParameters.Add("id",null);
                    var multiResult=          dbConnection.QueryMultiple(sql, dynamicParameters, transaction: dbTransaction);
                    var id= multiResult.Read().FirstOrDefault()?.id;

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    dbConnection.Execute(internalResult.Sql, t, transaction: dbTransaction);
                }
            }
           
            CloseDb();
            return t;
        }

        public List<T> Insert(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Insert(item);
            }
            CloseDb();
            this.uow.Commit();

            return list;
        }

        public List<T> GetAll()
        {
            var internalResult = InternalGetAll();
            OpenDb();
            var result = dbConnection.Query<T>(internalResult.Sql).ToList();
            CloseDb();

            return result;
        }

        public void Update(T t)
        {
            var internalResult = InternalUpdate(t);
          
            OpenDb();
            dbConnection.Execute(internalResult.Sql, t);
            CloseDb();
        }

        public void Delete(T t)
        {
            var internalResult = InternalDelete(t);
           
            OpenDb();
            dbConnection.Execute(internalResult.Sql, t);
            CloseDb();

        }

        public T Get(dynamic id)
        {
            DbQueryResult internalResult = InternalGet(id);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
          
            var result = dbConnection.QueryFirstOrDefault<T>(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }

        protected DynamicParameters ChangeDynamicParameters(List<SqlParameter> originSqlParameters)
        {
            if (originSqlParameters == null || originSqlParameters.Count == 0)
            {
                return null;
            }

            var result = new DynamicParameters();
            foreach (var parameter in originSqlParameters)
            {
                result.Add(parameter.ParameterName, parameter.Value);
            }

            return result;
        }
    }
}