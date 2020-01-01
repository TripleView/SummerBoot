using System.Data;
using System.Threading;
using SummerBoot.Core;

namespace SummerBoot.Repository.Druid
{
    public class DruidDbConnection : IDbConnection
    {
        public IDbConnection DbConnection {private set; get; }
        public DruidConnectionHolder Holder { private set; get; }

        public long ConnectedTimeNano { get; private set; }

        public bool Running { get; private set; }= false;

        public bool Disable { get; private set; } = false;
        public bool Abandoned { get; private set; } = false;
        public Thread OwnerThread { get; private set; }
        public long ConnectedTimeMillis { get; private set; }

        public void SetConnectedTimeNano()
        {
            if (ConnectedTimeNano <= 0) ConnectedTimeNano = SbUtil.NanoTime();
        }

        public void SetAbandond()
        {
            Abandoned = true;
        }
        
        public DruidDbConnection(DruidConnectionHolder holder)
        {
            DbConnection = holder.Connection;
            Holder = holder;
            OwnerThread=Thread.CurrentThread;
            ConnectedTimeMillis = SbUtil.CurrentTimeMillis();
        }

        public string ConnectionString { get => DbConnection.ConnectionString; set => this.DbConnection.ConnectionString=value; }

        public int ConnectionTimeout => this.DbConnection.ConnectionTimeout;

        public string Database => DbConnection.Database;

        public ConnectionState State => DbConnection.State;

        public IDbTransaction BeginTransaction()
        {
            return DbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return DbConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        { 
            DbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            Holder.DataSource.Recycle(this);
        }

        public IDbCommand CreateCommand()
        {
            var command= DbConnection.CreateCommand();
            
            return new DruidDbCommand(command);
        }

        public void Dispose()
        {
            DbConnection.Dispose();
        }

        public void Open()
        {
           DbConnection.Open();
        }
    }
}