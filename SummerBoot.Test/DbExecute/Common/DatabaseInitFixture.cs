using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SummerBoot.Test.DbExecute.Common.Db;

namespace SummerBoot.Test.DbExecute.Common;

public class DatabaseInitFixture : IDisposable
{
    public DatabaseInitFixture()
    {
      
    }

    private void InitDatabase()
    {
        InitMysqlDatabase();
        InitOracleDatabase();
        InitPgsqlDatabase();
        InitSqlserverDatabase();
    }
    private void InitMysqlDatabase()
    {
        //初始化数据库
        using (var database = new MysqlDb())    //新增
        {
            database.Database.EnsureDeleted();
            database.Database.EnsureCreated();
            database.Database.ExecuteSqlRaw("set global local_infile=1");
            database.Database.ExecuteSqlRaw(
                "ALTER TABLE nullabletable COMMENT = 'NullableTable'");
            database.Database.ExecuteSqlRaw(
                "ALTER TABLE nullabletable MODIFY COLUMN `Int2` int NULL COMMENT 'Int2'");
            database.Database.ExecuteSqlRaw(
                "ALTER TABLE nullabletable MODIFY COLUMN `Long2` bigint NULL COMMENT 'Long2'");

            database.Database.ExecuteSqlRaw(
                "ALTER TABLE notnullabletable COMMENT = 'NotNullableTable'");
            database.Database.ExecuteSqlRaw(
                "ALTER TABLE notnullabletable MODIFY COLUMN `Int2` int not NULL COMMENT 'Int2'");
            database.Database.ExecuteSqlRaw(
                "ALTER TABLE notnullabletable MODIFY COLUMN `Long2` bigint not NULL COMMENT 'Long2'");
            try
            {
                database.Database.ExecuteSqlRaw(
                    "drop TABLE test1.`CustomerWithSchema`");
            }
            catch (Exception e)
            {

            }
        }
    }

    private void InitPgsqlDatabase()
    {
        //初始化数据库
        using (var database = new PgsqlDb())    //新增
        {
            database.Database.EnsureDeleted();
            database.Database.EnsureCreated();
            database.Database.ExecuteSqlRaw(
                "create schema test1");
        }
    }

    private void InitOracleDatabase()
    {
        //??????????
        using (var database = new OracleDb())    //????
        {
            database.Database.EnsureDeleted();
            //Thread.Sleep(2000);
            database.Database.EnsureCreated();
            database.Database.ExecuteSqlRaw(
                "COMMENT ON TABLE NULLABLETABLE IS 'NullableTable'");
            database.Database.ExecuteSqlRaw(
                "COMMENT ON COLUMN NULLABLETABLE.INT2 IS 'Int2'");
            database.Database.ExecuteSqlRaw(
                "COMMENT ON COLUMN NULLABLETABLE.LONG2 IS 'Long2'");
            database.Database.ExecuteSqlRaw(
                "COMMENT ON TABLE NotNullableTable IS 'NotNullableTable'");
            database.Database.ExecuteSqlRaw(
                "COMMENT ON COLUMN NotNullableTable.INT2 IS 'Int2'");
            database.Database.ExecuteSqlRaw(
                "COMMENT ON COLUMN NotNullableTable.LONG2 IS 'Long2'");
            try
            {
                database.Database.ExecuteSqlRaw(
                    "drop TABLE TEST1.\"CUSTOMERWITHSCHEMA\"");
            }
            catch (Exception e)
            {

            }


        }
    }

    private void InitSqlserverDatabase()
    {
        //初始化数据库
        using (var database = new SqlServerDb())    //新增
        {

            database.Database.EnsureDeleted();
            database.Database.EnsureCreated();

            ExecuteRaw(database.Database, "drop TABLE TEST1.[CUSTOMERWITHSCHEMA]");

            ExecuteRaw(database.Database, "create SCHEMA test1");

            ExecuteRaw(database.Database, "create USER test WITH DEFAULT_SCHEMA = test1");

            ExecuteRaw(database.Database, "GRANT SELECT,INSERT,UPDATE,delete ON SCHEMA :: test1 TO test; ");

        }
    }

    private void ExecuteRaw(DatabaseFacade db, string sql)
    {
        try
        {

            db.ExecuteSqlRaw(
                sql);
        }
        catch (Exception e)
        {

        }
    }

    public void Dispose()
    {
       
    }
}