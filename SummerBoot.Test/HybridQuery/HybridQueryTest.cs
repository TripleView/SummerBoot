using SummerBoot.Test.Model;
using System;
using System.Threading.Tasks;
using SummerBoot.Test.Mysql.Models;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Test.Mysql.Repository;
using Microsoft.Extensions.Configuration;
using System.IO;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Mysql;
using SummerBoot.Test.Common;
using SummerBoot.Test.Oracle;

namespace SummerBoot.Test.HybridQuery;

public class HybridQueryTest
{
    private IServiceProvider serviceProvider;
   
    private void InitService(bool isTestRepeatedlyAddDatabaseUnit=false)
    {
        var build = new ConfigurationBuilder();
        build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
        build.AddJsonFile(ConstValue.CONFIG_FILE, true, true);
        var configurationRoot = build.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);
        services.AddSummerBoot();
        var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("mysql connectionString must not be null");
        }
        var oracleConnectionString = MyConfiguration.GetConfiguration("oracleDbConnectionString");
        if (string.IsNullOrWhiteSpace(oracleConnectionString))
        {
            throw new ArgumentNullException("oracle connectionString must not be null");
        }

        if (isTestRepeatedlyAddDatabaseUnit)
        {
            oracleConnectionString = connectionString;
        }
        services.AddSummerBootRepository(it =>
        {
            it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(connectionString,
                x =>
                {
                    x.BindRepositoriesWithAttribute<MysqlAutoRepositoryAttribute>();
                    x.BindDbGeneratorType<IDbGenerator1>();
                    x.BeforeInsert += new RepositoryEvent(entity =>
                    {
                        if (entity is GuidModel guidModel)
                        {
                            guidModel.Address = "abc";
                        }
                    });
                    x.BeforeUpdate += new RepositoryEvent(entity =>
                    {
                        if (entity is GuidModel guidModel)
                        {
                            guidModel.Address = "ppp";
                        }
                    });
                });

            it.AddDatabaseUnit<OracleConnection, IUnitOfWork2>(oracleConnectionString,
                x =>
                {
                    x.TableNameMapping = a => a.ToUpper();
                    x.ColumnNameMapping = a => a.ToUpper();
                    x.BindRepositoriesWithAttribute<OracleAutoRepositoryAttribute>();
                    x.BindManualRepositoriesWithAttribute<OracleManualRepositoryAttribute>();
                    x.BindDbGeneratorType<IDbGenerator2>();
                    x.BeforeInsert += new RepositoryEvent(entity =>
                    {
                        if (entity is GuidModel guidModel)
                        {
                            guidModel.Address = "abc";
                        }
                    });
                    x.BeforeUpdate += new RepositoryEvent(entity =>
                    {
                        if (entity is GuidModel guidModel)
                        {
                            guidModel.Address = "ppp";
                        }
                    });
                });
        });

        serviceProvider = services.BuildServiceProvider();
        serviceProvider = serviceProvider.CreateScope().ServiceProvider;
    }

    /// <summary>
    /// Testing mixed inserts and queries
    /// 测试混合插入和查询
    /// </summary>
    [Fact]
    public async Task TestHybridInsertAndQuery()
    {
        InitService();
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var mysqlOrderNo = Guid.NewGuid().ToString();
        var orderHeader = new OrderHeader();
        orderHeader.CreateTime = DateTime.UtcNow;
        orderHeader.CustomerId = 0;
        orderHeader.State = 1;
        orderHeader.OrderNo = mysqlOrderNo;
        await orderHeaderRepository.InsertAsync(orderHeader);

        var oracleOrderHeaderRepository = serviceProvider.GetService<SummerBoot.Test.Oracle.Repository.IOrderHeaderRepository>();
        var oracleOrderNo = Guid.NewGuid().ToString();
        var orderHeader2 = new SummerBoot.Test.Oracle.Models.OrderHeader();
        orderHeader2.CreateTime = DateTime.UtcNow;
        orderHeader2.CustomerId = 0;
        orderHeader2.State = 1;
        orderHeader2.OrderNo = oracleOrderNo;
        await oracleOrderHeaderRepository.InsertAsync(orderHeader2);
        var count1 =await orderHeaderRepository.CountAsync(x=>x.OrderNo== mysqlOrderNo);
        var count2 = await oracleOrderHeaderRepository.CountAsync(x => x.OrderNo == oracleOrderNo);

        Assert.True(count2==count1);
    }

    /// <summary>
    /// Testing mixed transactions
    /// 测试混合事务
    /// </summary>
    [Fact]
    public async Task TestHybridTransaction()
    {
        InitService();
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var uow1 = serviceProvider.GetService<IUnitOfWork1>();
        var mysqlOrderNo = Guid.NewGuid().ToString();
        var orderHeader = new OrderHeader();
        orderHeader.CreateTime = DateTime.UtcNow;
        orderHeader.CustomerId = 0;
        orderHeader.State = 1;
        orderHeader.OrderNo = mysqlOrderNo;
        uow1.BeginTransaction();
        await orderHeaderRepository.InsertAsync(orderHeader);

        var oracleOrderHeaderRepository = serviceProvider.GetService<SummerBoot.Test.Oracle.Repository.IOrderHeaderRepository>();
        var uow2 = serviceProvider.GetService<IUnitOfWork2>();
        var oracleOrderNo = Guid.NewGuid().ToString();
        var orderHeader2 = new SummerBoot.Test.Oracle.Models.OrderHeader();
        orderHeader2.CreateTime = DateTime.UtcNow;
        orderHeader2.CustomerId = 0;
        orderHeader2.State = 1;
        orderHeader2.OrderNo = oracleOrderNo;
        uow2.BeginTransaction();
        await oracleOrderHeaderRepository.InsertAsync(orderHeader2);

        uow1.Commit();
        try
        {
            throw new Exception("test");
            uow2.Commit();
        }
        catch (Exception e)
        {
          uow2.RollBack();
        }

        var count1 = await orderHeaderRepository.CountAsync(x => x.OrderNo == mysqlOrderNo);
        var count2 = await oracleOrderHeaderRepository.CountAsync(x => x.OrderNo == oracleOrderNo);

        Assert.Equal(1,count1);
        Assert.Equal(0, count2);
    }

    /// <summary>
    /// test Repeatedly add database unit
    /// 重复添加数据库单元
    /// </summary>
    [Fact]
    public async Task TestRepeatedlyAddDatabaseUnit()
    {
        Assert.Throws<RepeatAddDatabaseUnitException>(() =>
        {
            InitService(true);
        });
    }
}