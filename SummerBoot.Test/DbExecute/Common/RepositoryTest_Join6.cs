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
    public async Task TestJoinCount6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = -1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);

        var count = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .CountAsync(x => x.T1.Name == name);
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestInnerJoin6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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
        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = -1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var id = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .InnerJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T6.Name == name6)
            .OrderByDescending(x => x.T6.Id)
            .Select(x => x.T6.Id)
            .FirstOrDefaultAsync();

        Assert.Equal(joinTable6.Id, id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestRightJoin6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = -1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);

        var outputDto = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .RightJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .RightJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .RightJoin(joinTable6Repository, x => x.T5.OrderIndex == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T6.Name == name6)
            .OrderByDescending(x => x.T6.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id);
        Assert.Null(outputDto.Id2);
        Assert.Null(outputDto.Id3);
        Assert.Null(outputDto.Id4);
        Assert.Null(outputDto.Id5);

        var outputDto2 = await joinTable1Repository
            .RightJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .RightJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .RightJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .RightJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .RightJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T6.Name == name6)
            .OrderByDescending(x => x.T6.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id);
        Assert.Null(outputDto2.Id2);
        Assert.Null(outputDto2.Id3);
        Assert.Null(outputDto2.Id4);
        Assert.Equal(joinTable5.Id, outputDto2.Id5);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestLeftJoin6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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
        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = -1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var outputDto = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.OrderIndex == x.T2.Id)
            .LeftJoin(joinTable3Repository, x => x.T2.OrderIndex == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.OrderIndex == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.OrderIndex == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.OrderIndex == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T6.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id, Id6 = x.T6.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto.Id2);
        Assert.Null(outputDto.Id3);
        Assert.Null(outputDto.Id4);
        Assert.Null(outputDto.Id5);
        Assert.Null(outputDto.Id6);


        var outputDto2 = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.OrderIndex == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .OrderByDescending(x => x.T1.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id, Id2 = x.T2.Id, Id3 = x.T3.Id, Id4 = x.T4.Id, Id5 = x.T5.Id, Id6 = x.T6.Id })
            .FirstOrDefaultAsync();

        Assert.Null(outputDto2.Id6);
        Assert.Equal(joinTable2.Id, outputDto2.Id2);
        Assert.Equal(joinTable3.Id, outputDto2.Id3);
        Assert.Equal(joinTable4.Id, outputDto2.Id4);
        Assert.Equal(joinTable5.Id, outputDto2.Id5);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMax6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var joinTable6b = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 101,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6b);
        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MaxAsync(x => x.T6.OrderIndex);

        Assert.Equal(101, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMin6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var joinTable6b = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 100,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6b);


        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .MinAsync(x => x.T6.OrderIndex);

        Assert.Equal(1, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinSum6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var joinTable6b = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 100,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .SumAsync(x => x.T6.OrderIndex);

        Assert.Equal(101, value);

    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinAverage6Async(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var joinTable6b = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 101,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6b);

        var value = await joinTable1Repository
            .LeftJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .LeftJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .LeftJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .LeftJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .LeftJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id)
            .WhereIf(joinTable1.Id > 0, x => x.T1.Name == name)
            .AverageAsync(x => x.T6.OrderIndex);

        Assert.Equal(51, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithStringNullCallMethod6(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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

        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 1,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);
        var joinTable6b = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = 100,
            Table5Id = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6b);

        var test = new PropNullTest()
        {
            Name = null
        };
        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id)
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id)
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id)
            .InnerJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id && x.T5.Name == test.Name.Trim())
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
    public async Task TestJoinEntityWithNullableProperty6(DbType dbType)
    {
        ChangeDb(dbType);
        var joinTable1Repository = serviceProvider.GetService<IJoinTable1Repository>();
        var joinTable2Repository = serviceProvider.GetService<IJoinTable2Repository>();
        var joinTable3Repository = serviceProvider.GetService<IJoinTable3Repository>();
        var joinTable4Repository = serviceProvider.GetService<IJoinTable4Repository>();
        var joinTable5Repository = serviceProvider.GetService<IJoinTable5Repository>();
        var joinTable6Repository = serviceProvider.GetService<IJoinTable6Repository>();
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
        var name6 = GetRandomName();
        var joinTable6 = new JoinTable6()
        {
            CreateTime = DateTime.Now,
            Name = name6,
            OrderIndex = -1,
            Table5Id = joinTable5.Id,
            Table5Id2 = joinTable5.Id
        };
        await joinTable6Repository.InsertAsync(joinTable6);

        var result = await joinTable1Repository
            .InnerJoin(joinTable2Repository, x => x.T1.Id == x.T2.Table1Id)
            .InnerJoin(joinTable3Repository, x => x.T2.Id == x.T3.Table2Id2)
            .InnerJoin(joinTable4Repository, x => x.T3.Id == x.T4.Table3Id2)
            .InnerJoin(joinTable5Repository, x => x.T4.Id == x.T5.Table4Id2)
            .InnerJoin(joinTable6Repository, x => x.T5.Id == x.T6.Table5Id2)
            .Where(x => x.T1.Name == name)
            .Select(it => new { it.T1.Name, it.T3.Table2Id2, it.T4.Table3Id2, it.T5.Table4Id2, it.T6.Table5Id2 })
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(name, result.First().Name);
        Assert.Equal(joinTable2.Id, result.First().Table2Id2);
        Assert.Equal(joinTable3.Id, result.First().Table3Id2);
        Assert.Equal(joinTable4.Id, result.First().Table4Id2);
        Assert.Equal(joinTable5.Id, result.First().Table5Id2);
    }
}