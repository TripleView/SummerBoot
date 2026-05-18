using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlParser.Net;
using SummerBoot.Core;
using SummerBoot.Test.Common;
using System;
using System.IO;
using Xunit;

namespace SummerBoot.Test.DbGenerator;

public class DbGeneratorTest
{
    private IServiceProvider serviceProvider;
    private void InitService()
    {
        var build = new ConfigurationBuilder();
        build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
        build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
        var configurationRoot = build.Build();
        var services = new ServiceCollection();

        services.AddSummerBoot();
        services.AddSingleton<IConfiguration>(configurationRoot);
        var oracleDbConnectionString = MyConfiguration.GetConfiguration("oracleDbConnectionString");
        if (string.IsNullOrWhiteSpace(oracleDbConnectionString))
        {
            throw new ArgumentNullException("oracle connectionString must not be null");
        }

        var mysqlDbConnectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
        if (string.IsNullOrWhiteSpace(mysqlDbConnectionString))
        {
            throw new ArgumentNullException("mysql connectionString must not be null");
        }

        var sqlServerDbConnectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
        if (string.IsNullOrWhiteSpace(sqlServerDbConnectionString))
        {
            throw new ArgumentNullException("Sqlserver connectionString must not be null");
        }

        var pgsqlDbConnectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
        if (string.IsNullOrWhiteSpace(pgsqlDbConnectionString))
        {
            throw new ArgumentNullException("pgsql connectionString must not be null");
        }

        var sqliteDbConnectionString = $"Data Source=./{Guid.NewGuid().ToString()}.db";

        services.AddSummerBootRepository(it =>
        {
            //it.AddOracle<IUnitOfWork1>(oracleDbConnectionString,
            //    x =>
            //    {
            //        x.TableNameMapping = a => a.ToUpper();
            //        x.ColumnNameMapping = a => a.ToUpper();
            //        x.BindRepositoriesWithAttribute<OracleAutoRepositoryAttribute>();
            //        x.BindManualRepositoriesWithAttribute<OracleManualRepositoryAttribute>();
            //        x.BindDbGeneratorType<IDbGenerator1>();
            //    });

            //it.AddMysql<IUnitOfWork2>(mysqlDbConnectionString,
            //    x =>
            //    {
            //        x.BindRepositoriesWithAttribute<MysqlAutoRepositoryAttribute>();
            //        x.BindDbGeneratorType<IDbGenerator2>();

            //    });

            //it.AddSqlServer<IUnitOfWork3>(sqlServerDbConnectionString,
            //    x =>
            //    {
            //        x.BindRepositoriesWithAttribute<SqlServerAutoRepositoryAttribute>();
            //        x.BindDbGeneratorType<IDbGenerator3>();

            //    });

            //it.AddPgsql<IUnitOfWork4>(pgsqlDbConnectionString,
            //    x =>
            //    {
            //        x.BindRepositoriesWithAttribute<PgsqlAutoRepositoryAttribute>();
            //        x.BindDbGeneratorType<IDbGenerator4>();

            //    });

            //it.AddSqlite<IUnitOfWork5>(sqliteDbConnectionString,
            //    x =>
            //    {
            //        x.BindRepositoriesWithAttribute<SqliteAutoRepositoryAttribute>();
            //        x.BindDbGeneratorType<IDbGenerator5>();
            //    });
        });

        serviceProvider = services.BuildServiceProvider();
        serviceProvider = serviceProvider.CreateScope().ServiceProvider;
    }

    /// <summary>
    /// 测试增加字段设置默认值
    /// </summary>
    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Sqlite)]
    [InlineData(DbType.Oracle)]
    public void TestAddFieldsAndSetDefaultValues(DbType dbType)
    {
        InitService();
    }

}