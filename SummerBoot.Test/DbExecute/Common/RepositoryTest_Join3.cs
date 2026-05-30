using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Repository;
using SummerBoot.Test.DbExecute.Common.Dto;
using SummerBoot.Test.DbExecute.Common.Models;
using SummerBoot.Test.DbExecute.Common.Repository;
using System;
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
    public async Task TestJoinCount3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = 1
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = 1,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var count = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository,x=>x.T2.Id==x.T3.Table2Id)
            .CountAsync(x => x.T1.Name== name);
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestInnerJoin3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = 1
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = 1,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);
        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var id = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T3.Name == name3)
            .OrderByDescending(x => x.T3.Id)
            .Select(x => x.T3.Id)
            .FirstOrDefaultAsync();
      
        Assert.Equal(joinTable3.Id, id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestRightJoin3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);
        var outputDto = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T3.Name == name3)
            .OrderByDescending(x => x.T3.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id,Id2 = x.T2.Id})
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id);
        Assert.Null(outputDto.Id2);

        var outputDto2 = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T3.Name == name3)
            .OrderByDescending(x => x.T3.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id);
        Assert.Equal(joinTable2.Id, outputDto2.Id2);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestLeftJoin3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);
        var outputDto = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .LeftJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T3.Id)
            .Select(x => new JoinOutputDto() { Id = x.T3.Id, Id2 = x.T2.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id);
        Assert.Null(outputDto.Id2);

        var outputDto2 = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T1.Id)
            .Select(x => new JoinOutputDto() { Id = x.T3.Id, Id2 = x.T2.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id);
        Assert.Equal(joinTable2.Id, outputDto2.Id2);

    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMax3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 101,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var joinTable3b = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MaxAsync(x => x.T3.OrderIndex);

        Assert.Equal(101, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMin3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 100,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var joinTable3b = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3b);


        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MinAsync(x => x.T3.OrderIndex);

        Assert.Equal(1, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinSum3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 100,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var joinTable3b = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3b);


        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .SumAsync(x => x.T3.OrderIndex);

        Assert.Equal(101, value);

    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinAverage3Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 101,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var joinTable3b = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .AverageAsync(x => x.T3.OrderIndex);

        Assert.Equal(51, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithStringNullCallMethod3(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 101,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var joinTable3b = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 1,
            Table2Id = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3b);
        var test = new PropNullTest()
        {
            Name = null
        };
        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id && x.T2.Name == test.Name.Trim())
            .Select(it => new { it.T1.Name, it.T2.Table1Id })
            .ToListAsync();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithNullableProperty3(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var name = GetRandomName();
        var joinTable1 = new JoinTable1()
        {
            CreateTime = DateTime.Now,
            Name = name,
            OrderIndex = -100
        };
        await joinTable1Repository.InsertAsync(joinTable1);

        var joinTable2 = new JoinTable2()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = -100,
            Table1Id = joinTable1.Id
        };
        await joinTable2Repository.InsertAsync(joinTable2);

        var name3 = GetRandomName();
        var joinTable3 = new JoinTable3()
        {
            CreateTime = DateTime.Now,
            Name = name3,
            OrderIndex = 101,
            Table2Id2 = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);

        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id2)
            .Where(x=>x.T1.Name==name)
            .Select(it => new { it.T1.Name, it.T3.Table2Id2 })
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(name, result.First().Name);
        Assert.Equal(joinTable2.Id, result.First().Table2Id2);
    }
}