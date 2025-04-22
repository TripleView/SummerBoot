using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Polly.Caching;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Performance.Test;

public class Performance
{
    private int total = 20000;
    private IFreeSql fsql;
    static readonly string CONFIG_FILE = "app.json";  // 配置文件地址
    private IServiceProvider serviceProvider;

    private void InitFreeSql()
    {
        var build = new ConfigurationBuilder();
        build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
        build.AddJsonFile(CONFIG_FILE, true, true);
        var configurationRoot = build.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);
        var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
        this.fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.MySql, connectionString)
            .UseAutoSyncStructure(true)
            .Build(); //be sure to define as singleton mode


    }

    [Benchmark()]
    public async Task TestFreeSqlBatchInsert()
    {
        InitFreeSql();
        var nullableTableList = new List<NullableTableForFreeSql>();
        //var total = 200000;
        for (int i = 0; i < total; i++)
        {
            var a = new NullableTableForFreeSql()
            {
                Int2 = 2,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = DateTime.Now,
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = Guid.NewGuid(),
                Id = 1,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2,
                Enum2 = Enum2.y,
                Int3 = 4
            };
            nullableTableList.Add(a);
        }

        var sw = new Stopwatch();
        sw.Start();
        //var d= fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteAffrows();
        fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteMySqlBulkCopy();
        sw.Stop();
        Console.WriteLine("TestFreeSqlBatchInsert:" + sw.ElapsedMilliseconds);
    }

    [Benchmark()]
    public async Task TestFreeSqlSelect()
    {
        InitFreeSql();
        var sw = new Stopwatch();
        sw.Start();
        //var d= fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteAffrows();
        var result = await fsql.Select<NullableTableForFreeSql>().ToListAsync();
        sw.Stop();
        Console.WriteLine("TestFreeSqlSelect:" + sw.ElapsedMilliseconds);
    }

    [Benchmark()]
    public async Task TestFreeSqlDelete()
    {
        InitFreeSql();
        var sw = new Stopwatch();
        sw.Start();
        //var d= fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteAffrows();
        var result = await fsql.Delete<NullableTableForFreeSql>().Where(a => true).ExecuteAffrowsAsync();
        sw.Stop();
        Console.WriteLine("TestFreeSqlDelete:" + sw.ElapsedMilliseconds);
    }

    [Benchmark()]
    public async Task TestSummerBootDelete()
    {
        InitSummerboot();
        var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
        var sw = new Stopwatch();
        sw.Start();
        //var d= fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteAffrows();
        var result = await nullableTableRepository.DeleteAsync(it => true);
        sw.Stop();
        Console.WriteLine("TestSummerBootDelete:" + sw.ElapsedMilliseconds);
    }

    [Benchmark()]
    public async Task TestSummerBootSelect()
    {
        InitSummerboot();
        var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
        var sw = new Stopwatch();
        sw.Start();
        //var d= fsql.Insert<NullableTableForFreeSql>().AppendData(nullableTableList).ExecuteAffrows();
        var result = await nullableTableRepository.GetAllAsync();
        sw.Stop();
        Console.WriteLine("TestSummerBootSelect:" + sw.ElapsedMilliseconds);
    }

    [Benchmark()]
    public async Task TestSummerbootBatchInsert()
    {
        InitSummerboot();
        var uow = serviceProvider.GetService<IUnitOfWork1>();
        var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
        var dbGenerate = serviceProvider.GetService<IDbGenerator1>();
        var sqlResult = dbGenerate.GenerateSql(new List<Type>() { typeof(NullableTable) });
        foreach (var generateDatabaseSqlResult in sqlResult)
        {
            dbGenerate.ExecuteGenerateSql(generateDatabaseSqlResult);
        }
        var nullableTableList = new List<NullableTable>();
        //var total = 200000;
        for (int i = 0; i < total; i++)
        {
            var a = new NullableTable()
            {
                Int2 = 2,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = DateTime.Now,
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = Guid.NewGuid(),
                Id = 1,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2,
                Enum2 = Enum2.y,
                Int3 = 4
            };
            nullableTableList.Add(a);
        }
        var sw = new Stopwatch();
        sw.Start();
        await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
        //await nullableTableRepository.InsertAsync(nullableTableList);
        sw.Stop();
        Console.WriteLine("TestSummerbootBatchInsert:" + sw.ElapsedMilliseconds);
    }

    private void InitSummerboot()
    {
        var build = new ConfigurationBuilder();
        build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
        build.AddJsonFile(CONFIG_FILE, true, true);
        var configurationRoot = build.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);
        services.AddSummerBoot();
        var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("mysql connectionString must not be null");
        }

        services.AddSummerBootRepository(it =>
        {
            it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(connectionString,
                x =>
                {
                    x.BindRepositoriesWithAttribute<AutoRepository1Attribute>();
                    x.BindDbGeneratorType<IDbGenerator1>();
                    x.BeforeInsert += new RepositoryEvent(entity =>
                    {

                    });
                    x.BeforeUpdate += new RepositoryEvent(entity =>
                    {

                    });
                });
        });

        serviceProvider = services.BuildServiceProvider();
        serviceProvider = serviceProvider.CreateScope().ServiceProvider;
    }
}