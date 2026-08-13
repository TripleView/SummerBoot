using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Repository;
using SummerBoot.Test.DbExecute.Common.Dto;
using SummerBoot.Test.DbExecute.Common.Models;
using SummerBoot.Test.DbExecute.Common.Repository;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Test.DbExecute.Common;

public partial class RepositoryTest
{
    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task ConcurrentTestAsync1(DbType dbType)
    {
        const int concurrentCount = 50;
        for (int i = 0; i < 5; i++)
        {
            var tasks = Enumerable.Range(1, concurrentCount)
                .Select(_ => Task.Run(() => InternalConcurrentTestAsync1(dbType)))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    public async Task InternalConcurrentTestAsync1(DbType dbType)
    {
        var sw = Stopwatch.StartNew();
        ChangeDb(dbType);
        var tempServiceProvider = rootServiceProvider.CreateScope().ServiceProvider;
        var joinTable1Repository = tempServiceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = tempServiceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = tempServiceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = tempServiceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = tempServiceProvider.GetService<IJoinTable5Repository>();

        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var name2 = GetRandomName();
        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = name2,
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);
 
        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = -1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);
   
        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = -1,
            Table3Id = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);

        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = -1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var outputDto = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .RightJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .RightJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T5.Name == name5)
            .OrderByDescending(x => x.T5.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id);
        Assert.Null(outputDto.Id2);
        Assert.Null(outputDto.Id3);
        Assert.Null(outputDto.Id4);

        var outputDto2 = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .RightJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .RightJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T5.Name == name5)
            .OrderByDescending(x => x.T5.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id);
        Assert.Null(outputDto2.Id2);
        Assert.Null(outputDto2.Id3);
        Assert.Equal(joinTable4.Id, outputDto2.Id4);
        sw.Stop();
        output.WriteLine("≤‚ ‘ÕÍ¡À," + sw.ElapsedMilliseconds);
    }
}