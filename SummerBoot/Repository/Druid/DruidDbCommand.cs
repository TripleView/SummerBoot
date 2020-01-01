using System.Data;
using System.Data.Common;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace SummerBoot.Repository.Druid
{
    public class DruidDbCommand : IDbCommand
    {
        private IDbCommand DbCommand { set; get; }


        public DruidDbCommand(IDbCommand dbCommand)
        {
            DbCommand = dbCommand;
        }

        public string CommandText { get => DbCommand.CommandText; set => DbCommand.CommandText=value; }
        public int CommandTimeout { get => DbCommand.CommandTimeout; set => DbCommand.CommandTimeout=value; }
        public CommandType CommandType { get => DbCommand.CommandType; set => DbCommand.CommandType=value; }
        public IDbConnection Connection { get => DbCommand.Connection; set => DbCommand.Connection=value; }

        public IDataParameterCollection Parameters => new DruidDataParameterCollection(DbCommand.Parameters);

        public IDbTransaction Transaction { get => DbCommand.Transaction; set => DbCommand.Transaction=value; }
        public UpdateRowSource UpdatedRowSource { get => DbCommand.UpdatedRowSource; set => DbCommand.UpdatedRowSource=value; }

        public void Cancel()
        {
            DbCommand.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return DbCommand.CreateParameter();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return DbCommand.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return DbCommand.ExecuteReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return DbCommand.ExecuteReader(behavior);
        }

        public object ExecuteScalar()
        {
            return DbCommand.ExecuteScalar();
        }

        public void Prepare()
        {
            DbCommand.Prepare();
        }
    }
}