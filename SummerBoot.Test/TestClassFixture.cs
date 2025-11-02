using Microsoft.EntityFrameworkCore;
using SummerBoot.Test.Oracle.Db;

namespace SummerBoot.Test;

using Xunit;
using System;

// 1. 定义夹具类（存放初始化逻辑）
public class TestClassFixture : IDisposable
{
    // 构造函数：测试类第一次执行前调用（仅一次）
    public TestClassFixture()
    {
        Console.WriteLine("测试类初始化（所有测试方法执行前）");
        // 例如：创建数据库连接、初始化全局配置等
    }

    public void InitOracleDatabase()
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

    // 清理逻辑：测试类所有方法执行完毕后调用
    public void Dispose()
    {
        Console.WriteLine("测试类清理（所有测试方法执行后）");
        // 例如：关闭数据库连接、释放资源等
    }

    // 可选：提供共享资源或方法给测试类使用
    public string GetSharedData() => "共享的数据";
}
