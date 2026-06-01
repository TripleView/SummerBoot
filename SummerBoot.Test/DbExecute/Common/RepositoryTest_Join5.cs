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
    public async Task TestJoinCount5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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

        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = "ABC",
            OrderIndex = 1,
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

        var count = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .CountAsync(x => x.T1.Name == name);
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestInnerJoin5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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
        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = 1,
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

        var id = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T5.Name == name5)
            .OrderByDescending(x => x.T5.Id)
            .Select(x => x.T5.Id)
            .FirstOrDefaultAsync();

        Assert.Equal(joinTable5.Id, id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestRightJoin5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestLeftJoin5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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
            .LeftJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .LeftJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T5.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id2);
        Assert.Null(outputDto.Id3);
        Assert.Null(outputDto.Id4);
        Assert.Null(outputDto.Id5);

        var outputDto2 = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T1.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id5);
        Assert.Equal(joinTable2.Id, outputDto2.Id2);
        Assert.Equal(joinTable3.Id, outputDto2.Id3);
        Assert.Equal(joinTable4.Id, outputDto2.Id4);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMax5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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
            OrderIndex = 1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var joinTable5b = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex =101,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MaxAsync(x => x.T5.OrderIndex);

        Assert.Equal(101, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMin5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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

        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = 1,
            Table3Id = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);


        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var joinTable5b = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 101,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5b);


        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MinAsync(x => x.T5.OrderIndex);

        Assert.Equal(1, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinSum5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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

        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = 1,
            Table3Id = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);

        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var joinTable5b = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 100,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .SumAsync(x => x.T5.OrderIndex);

        Assert.Equal(101, value);

    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinAverage5Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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

        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = 1,
            Table3Id = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);

        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var joinTable5b = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 101,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .AverageAsync(x => x.T5.OrderIndex);

        Assert.Equal(51, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithStringNullCallMethod5(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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

        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = 1,
            Table3Id = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);


        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 1,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var joinTable5b = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = 101,
            Table4Id = joinTable4.Id
        };
        await joinTable5Repository.InsertAsync(joinTable5b);

        var test = new PropNullTest()
        {
            Name = null
        };
        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id )
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id )
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id && x.T5.Name == test.Name.Trim())
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
    public async Task TestJoinEntityWithNullableProperty5(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
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
            Table2Id = joinTable2.Id,
            Table2Id2 = joinTable2.Id
        };
        await joinTable3Repository.InsertAsync(joinTable3);
        var name4 = GetRandomName();
        var joinTable4 = new JoinTable4()
        {
            CreateTime = DateTime.Now,
            Name = name4,
            OrderIndex = -1,
            Table3Id = joinTable3.Id,
            Table3Id2 = joinTable3.Id
        };
        await joinTable4Repository.InsertAsync(joinTable4);

        var name5 = GetRandomName();
        var joinTable5 = new JoinTable5()
        {
            CreateTime = DateTime.Now,
            Name = name5,
            OrderIndex = -1,
            Table4Id = joinTable4.Id,
            Table4Id2 = joinTable4.Id,
        };
        await joinTable5Repository.InsertAsync(joinTable5);

        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id2)
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id2)
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id2)
            .Where(x => x.T1.Name == name)
            .Select(it => new { it.T1.Name, it.T3.Table2Id2, it.T4.Table3Id2, it.T5.Table4Id2 })
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(name, result.First().Name);
        Assert.Equal(joinTable2.Id, result.First().Table2Id2);
        Assert.Equal(joinTable3.Id, result.First().Table3Id2);
        Assert.Equal(joinTable4.Id, result.First().Table4Id2);
    }
}