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
    public async Task TestJoinCountAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = "ABC",
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 1
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var count = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .CountAsync(x => x.T1.Id == orderHeader.Id);
        Assert.Equal(2, count);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestInnerJoinAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var name = GetRandomName();
        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = name,
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 1
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var id = await orderHeaderRepository
            .InnerJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .WhereIf(orderHeader.Id > 0, x => x.T1.OrderNo == name)
            .OrderByDescending(x => x.T2.Id)
            .Select(x => x.T2.Id)
            .FirstOrDefaultAsync();
        Assert.Equal(orderDetail2.Id, id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestRightJoinAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var name = GetRandomName();
        var name2 = GetRandomName();
        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = name,
            State = -100
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = name2,
            Quantity = 2,
            State = 1
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var outputDto = await orderHeaderRepository
            .RightJoin(orderDetailRepository, x => x.T1.State == x.T2.OrderHeaderId)
            .WhereIf(orderHeader.Id > 0, x => x.T2.ProductName == name2)
            .OrderByDescending(x => x.T2.Id)
            .Select(x => new JoinOutputDto() { Id = x.T1.Id })
            .FirstOrDefaultAsync();
        Assert.Null(outputDto.Id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestLeftJoinAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var name = GetRandomName();
        var name2 = GetRandomName();
        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = name,
            State = 100
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderHeader2 = new OrderHeader()
        {
            CreateTime = DateTime.Now.AddMinutes(1),
            OrderNo = name,
            State = 100
        };
        await orderHeaderRepository.InsertAsync(orderHeader2);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        await orderDetailRepository.InsertAsync(orderDetail1);

        var outputDto = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .Where(x => x.T1.OrderNo == name)
            .OrderBy(x => x.T1.State)
            .ThenByDescending(x => x.T1.CreateTime)
            .Select(x => new JoinOutputDto() { Id = x.T2.Id })
            .FirstOrDefaultAsync();
        Assert.Null(outputDto.Id);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMaxAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = "ABC",
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 100
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var value = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .Where(x => x.T1.Id == orderHeader.Id)
            .MaxAsync(x => x.T2.State);
        Assert.Equal(100, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinMinAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = "ABC",
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 100
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var value = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .Where(x => x.T1.Id == orderHeader.Id)
            .MinAsync(x => x.T2.State);
        Assert.Equal(1, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinSumAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = "ABC",
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 100
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var value = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .Where(x => x.T1.Id == orderHeader.Id)
            .SumAsync(x => x.T2.State);
        Assert.Equal(101, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinAverageAsync(DbType dbType)
    {
        ChangeDb(dbType);
        var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

        var orderHeader = new OrderHeader()
        {
            CreateTime = DateTime.Now,
            OrderNo = "ABC",
            State = 1
        };
        await orderHeaderRepository.InsertAsync(orderHeader);

        var orderDetail1 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "A",
            Quantity = 1,
            State = 1
        };

        var orderDetail2 = new OrderDetail()
        {
            OrderHeaderId = orderHeader.Id,
            ProductName = "B",
            Quantity = 2,
            State = 100
        };
        await orderDetailRepository.InsertAsync(orderDetail1);
        await orderDetailRepository.InsertAsync(orderDetail2);

        var value = await orderHeaderRepository
            .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            .Where(x => x.T1.Id == orderHeader.Id)
            .AverageAsync(x => x.T2.State);
        Assert.Equal(50, value);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithStringNullCallMethod(DbType dbType)
    {
        ChangeDb(dbType);
        var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
        var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
        var name = GetRandomName();
        var propNullTest = new PropNullTest()
        {
            Name = name
        };
        await propNullTestRepository.InsertAsync(propNullTest);
        var propNullTestItem = new PropNullTestItem()
        {
            Name = "testitem",
            MapId = propNullTest.Id
        };
        await propNullTestItemRepository.InsertAsync(propNullTestItem);

        var test = new PropNullTest()
        {
            Name = null
        };

        var result = await propNullTestRepository.InnerJoin(propNullTestItemRepository, it => it.T1.Id == it.T2.MapId && it.T2.Name == test.Name.Trim())
            .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Pgsql)]
    [InlineData(DbType.Oracle)]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.Sqlite)]
    public async Task TestJoinEntityWithNullableProperty(DbType dbType)
    {
        ChangeDb(dbType);
        var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
        var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
        var name = GetRandomName();
        var propNullTest = new PropNullTest()
        {
            Name = name
        };
        await propNullTestRepository.InsertAsync(propNullTest);
        var propNullTestItem = new PropNullTestItem()
        {
            Name = "testitem",
            MapId = propNullTest.Id
        };
        await propNullTestItemRepository.InsertAsync(propNullTestItem);

        var result = await propNullTestRepository.LeftJoin(propNullTestItemRepository, it => it.T1.Id == it.T2.MapId)
            .Where(x => x.T1.Name == name)
            .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
        Assert.Single(result);
        Assert.Equal(name, result.First().Name);
        Assert.Equal(propNullTest.Id, result.First().MapId);
    }
}