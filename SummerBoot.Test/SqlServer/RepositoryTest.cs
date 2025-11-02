using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Common;
using SummerBoot.Test.Common.Dto;
using SummerBoot.Test.SqlServer.Db;
using SummerBoot.Test.SqlServer.Dto;
using SummerBoot.Test.SqlServer.Models;
using SummerBoot.Test.SqlServer.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.SqlServer
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;
        /// <summary>
        /// 测试left/right join时左右实体类里有string类型,并且string类型为null时调用trim方法
        /// </summary>
        //[Fact, Priority(426)]
        //public async Task TestJoinEntityWithStringNullCallMethod()
        //{
        //    InitDatabase();

        //    var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
        //    var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
        //    var propNullTest = new PropNullTest()
        //    {
        //        Name = "test"
        //    };
        //    await propNullTestRepository.InsertAsync(propNullTest);
        //    var propNullTestItem = new PropNullTestItem()
        //    {
        //        Name = "testitem",
        //        MapId = propNullTest.Id
        //    };
        //    await propNullTestItemRepository.InsertAsync(propNullTestItem);

        //    var test = new PropNullTest()
        //    {
        //        Name = null
        //    };

        //    var result = await propNullTestRepository.InnerJoin(new PropNullTestItem(), it => it.T1.Id == it.T2.MapId && it.T2.Name == test.Name.Trim())
        //        .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
        //    Assert.Empty(result);

        //}


        /// <summary>
        /// 测试left/right join时左右实体类里连接属性为可空类型 
        /// </summary>
        //[Fact, Priority(425)]
        //public async Task TestJoinEntityWithNullableProperty()
        //{
        //    InitDatabase();

        //    var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
        //    var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
        //    var propNullTest = new PropNullTest()
        //    {
        //        Name = "test"
        //    };
        //    await propNullTestRepository.InsertAsync(propNullTest);
        //    var propNullTestItem = new PropNullTestItem()
        //    {
        //        Name = "testitem",
        //        MapId = propNullTest.Id
        //    };
        //    await propNullTestItemRepository.InsertAsync(propNullTestItem);

        //    var result = await propNullTestRepository.LeftJoin(new PropNullTestItem(), it => it.T1.Id == it.T2.MapId)
        //        .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
        //    Assert.Single(result);
        //    Assert.Equal("test", result.First().Name);
        //    Assert.Equal(1, result.First().MapId);
        //}

        /// <summary>
        /// 测试where条件中参数包含方法
        /// </summary>
        [Fact, Priority(424)]
        public async Task TestWhereConditionContainOtherMethod()
        {
            InitDatabase();

            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var nullableTableList = new List<NullableTable>();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0);
            for (int i = 0; i < 5; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = i,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = dateNow.AddMinutes(i),
                    Decimal2 = 1m,
                    Decimal3 = 1.1m,
                    Double2 = 1.1,
                    Float2 = (float)1.1,
                    Guid2 = Guid.NewGuid(),
                    Short2 = 1,
                    TimeSpan2 = TimeSpan.FromHours(1),
                    String2 = "sb",
                    String3 = "sb",
                    Long2 = 2
                };

                nullableTableList.Add(a);
            }

            await nullableTableRepository.InsertAsync(nullableTableList);
            var result = nullableTableRepository.Where(it => it.DateTime2 >= dateNow.AddMinutes(3)).ToList();
            Assert.Equal(2, result.Count);

            var result2 = nullableTableRepository.Where(it => it.DateTime2 >= TestWhereConditionContainDateTimeMethodItem(dateNow)).ToList();
            Assert.Equal(2, result2.Count);
        }

        private DateTime TestWhereConditionContainDateTimeMethodItem(DateTime dateTime)
        {
            return dateTime.AddMinutes(3);
        }
        /// <summary>
        /// 测试where条件中参数是可空类型
        /// </summary>
        [Fact, Priority(423)]
        public async Task TestWhereConditionHaveNullableValue()
        {
            InitDatabase();

            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var nullableTableList = new List<NullableTable>();

            for (int i = 0; i < 5; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = i,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = DateTime.Now,
                    Decimal2 = 1m,
                    Decimal3 = 1.1m,
                    Double2 = 1.1,
                    Float2 = (float)1.1,
                    Guid2 = Guid.NewGuid(),
                    Short2 = 1,
                    TimeSpan2 = TimeSpan.FromHours(1),
                    String2 = "sb",
                    String3 = "sb",
                    Long2 = 2,
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };

                nullableTableList.Add(a);
            }

            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
            var list = new List<int>() { 1, 11, 111 };
            var result = nullableTableRepository.Where(it => list.Contains(it.Int2.Value)).ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal(1, result[0].Int2);
            var result2 = nullableTableRepository.Where(it => it.Int2.Value == new { value = 2 }.value).ToList();
            Assert.Equal(1, result2.Count);
            Assert.Equal(2, result2[0].Int2);
        }

        /// <summary>
        /// 测试where条件的各种情况
        /// </summary>
        //[Fact, Priority(422)]
        //public async Task TestWhereCombine()
        //{
        //    InitDatabase();

        //    var addressRepository = serviceProvider.GetService<IAddressRepository>();
        //    var customerRepository = serviceProvider.GetService<ICustomerRepository>();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var birthDay = new DateTime(1992, 2, 15);
        //    var customer1 = new Customer()
        //    {
        //        Age = 1,
        //        Name = "bob",
        //        CustomerNo = "A1",
        //        TotalConsumptionAmount = 1,
        //        BirthDay = birthDay
        //    };

        //    var customer2 = new Customer()
        //    {
        //        Age = 2,
        //        Name = "bob",
        //        CustomerNo = "A2",
        //        TotalConsumptionAmount = 2
        //    };
        //    await customerRepository.InsertAsync(customer1);
        //    await customerRepository.InsertAsync(customer2);

        //    #region 测试单表where

        //    var customerList1 = await customerRepository.Where(it => it.Name == "bob").ToListAsync();
        //    Assert.Equal(2, customerList1.Count);
        //    var customerList2 = await customerRepository.Where(it => it.Name == "bob").Where(it => it.Age == 1).ToListAsync();
        //    Assert.Equal(1, customerList2.Count);

        //    var customerList3 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhere(it => it.Age == 1).ToListAsync();
        //    Assert.Equal(2, customerList3.Count);

        //    var customerList4 = await customerRepository.WhereIf(true, it => it.CustomerNo == "A2").ToListAsync();
        //    Assert.Equal(1, customerList4.Count);

        //    var customerList5 = await customerRepository.WhereIf(false, it => it.CustomerNo == "A2").ToListAsync();
        //    Assert.Equal(2, customerList5.Count);

        //    var customerList6 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhereIf(true, it => it.Age == 1).ToListAsync();
        //    Assert.Equal(2, customerList6.Count);

        //    var customerList7 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhereIf(false, it => it.Age == 1).ToListAsync();
        //    Assert.Equal(1, customerList7.Count);

        //    var customerList8 = await customerRepository.OrWhere(it => it.CustomerNo == "A2").ToListAsync();
        //    Assert.Equal(1, customerList8.Count);

        //    var newTestCustomer = new Customer()
        //    {
        //        CustomerNo = " A2 "
        //    };
        //    var customerList9 = await customerRepository.Where(it => it.CustomerNo.Contains(newTestCustomer.CustomerNo.Trim())).ToListAsync();
        //    Assert.Equal(1, customerList9.Count);
        //    #endregion


        //    var baseTime = new DateTime(2023, 10, 24, 17, 0, 0);
        //    var date1 = baseTime.AddMinutes(-10);
        //    var date2 = baseTime.AddMinutes(-5);
        //    var date3 = baseTime.AddMinutes(-7);
        //    var date4 = baseTime.AddMinutes(-12);
        //    var address1 = new Address()
        //    {
        //        CustomerId = customer1.Id,
        //        City = "A",
        //        CreateOn = date3
        //    };
        //    var address2 = new Address()
        //    {
        //        CustomerId = customer1.Id,
        //        City = "B",
        //        CreateOn = date4
        //    };
        //    var address3 = new Address()
        //    {
        //        CustomerId = customer2.Id,
        //        City = "A",
        //        CreateOn = date4
        //    };
        //    await addressRepository.InsertAsync(address1);
        //    await addressRepository.InsertAsync(address2);
        //    await addressRepository.InsertAsync(address3);

        //    var firstBuyDate = new DateTime(2023, 8, 1, 17, 0, 0);
        //    var secondBuyDate = new DateTime(2023, 10, 26, 17, 0, 0);

        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = firstBuyDate,
        //        CustomerId = customer1.Id,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader1 = new OrderHeader()
        //    {
        //        CreateTime = secondBuyDate,
        //        CustomerId = customer1.Id,
        //        OrderNo = "JJJ",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader1);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = firstBuyDate,
        //        CustomerId = customer2.Id,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };

        //    var orderDetail3 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader2.Id,
        //        ProductName = "C",
        //        Quantity = 3,
        //        State = 1
        //    };

        //    var orderDetail4 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader1.Id,
        //        ProductName = "D",
        //        Quantity = 4,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);
        //    await orderDetailRepository.InsertAsync(orderDetail3);
        //    await orderDetailRepository.InsertAsync(orderDetail4);
        //    #region 测试双表where

        //    var result01 = await customerRepository
        //        .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId && it.T2.City==new {city="B"}.city)
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(1, result01.Count);

        //    var result02 = await customerRepository
        //        .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId )
        //        .Where(it=>it.T2.City == new { city = "B" }.city)
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(1, result02.Count);

        //    var result03 = await customerRepository
        //        .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T2.City == new { city = "B" }.city)
        //        .Select(it => it.T1.CustomerNo)
        //        .ToListAsync();
        //    Assert.Equal(1, result03.Count);

        //    var result04 = await customerRepository
        //        .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T2.City == new { city = "B" }.city)
        //        .Select(it => new { it.T1.BirthDay, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(1, result04.Count);
        //    Assert.Equal(birthDay, result04[0].BirthDay);

        //    var result = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T2.City == "A")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result.Count);

        //    var result11 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .OrWhere(it => it.T2.City == "A")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result11.Count);

        //    var result111 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId && it.T2.City.StartsWith("A"))
        //        .OrWhere(it => it.T2.City == "A")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result111.Count);

        //    var result2 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T2.City == "A")
        //        .Where(it => it.T1.CustomerNo == "A1")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(1, result2.Count);

        //    var result3 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .WhereIf(true, it => it.T2.City == "A")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result3.Count);

        //    var result4 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .WhereIf(false, it => it.T2.City == "A")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(3, result4.Count);

        //    var result5 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T1.CustomerNo == "A2")
        //        .OrWhere(it => it.T2.City == "B")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result5.Count);

        //    var result6 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T1.CustomerNo == "A2")
        //        .OrWhereIf(true, it => it.T2.City == "B")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(2, result6.Count);

        //    var result7 = await customerRepository
        //        .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
        //        .Where(it => it.T1.CustomerNo == "A2")
        //        .OrWhereIf(false, it => it.T2.City == "B")
        //        .Select(it => new { it.T1.CustomerNo, it.T2.City })
        //        .ToListAsync();
        //    Assert.Equal(1, result7.Count);
        //    #endregion

        //    #region 测试3表where
        //    //测试多join和where条件
        //    //  .InnerJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //    var orderList = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList.Count);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("D", orderList[2].ProductName);

        //    var orderList1 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T3.CustomerNo == "A1")
        //        .Where(it => it.T1.CreateTime == firstBuyDate)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(2, orderList1.Count);
        //    Assert.Equal("A", orderList1[0].ProductName);
        //    Assert.Equal("B", orderList1[1].ProductName);


        //    var orderList3 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhere(it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList3.Count);
        //    Assert.Equal("A", orderList3[0].ProductName);
        //    Assert.Equal("B", orderList3[1].ProductName);
        //    Assert.Equal("D", orderList3[2].ProductName);

        //    var orderList4 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .WhereIf(true, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList4.Count);
        //    Assert.Equal("A", orderList4[0].ProductName);
        //    Assert.Equal("B", orderList4[1].ProductName);
        //    Assert.Equal("D", orderList4[2].ProductName);

        //    var orderList5 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .WhereIf(false, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(4, orderList5.Count);

        //    var orderList6 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhereIf(true, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList6.Count);
        //    Assert.Equal("A", orderList3[0].ProductName);
        //    Assert.Equal("B", orderList3[1].ProductName);
        //    Assert.Equal("D", orderList3[2].ProductName);

        //    var orderList7 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhereIf(false, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(4, orderList7.Count);
        //    Assert.Equal("A", orderList7[0].ProductName);
        //    Assert.Equal("B", orderList7[1].ProductName);
        //    Assert.Equal("C", orderList7[2].ProductName);
        //    Assert.Equal("D", orderList7[3].ProductName);
        //    #endregion

        //    #region 测试4表where
        //    //测试多join和where条件
        //    //  .InnerJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //    var orderList8 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList8.Count);
        //    Assert.Equal("A", orderList8[0].ProductName);
        //    Assert.Equal("A", orderList8[0].CustomerCity);
        //    Assert.Equal("B", orderList8[1].ProductName);
        //    Assert.Equal("D", orderList8[2].ProductName);

        //    var orderList9 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T3.CustomerNo == "A1")
        //        .Where(it => it.T1.CreateTime == firstBuyDate)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(2, orderList9.Count);
        //    Assert.Equal("A", orderList9[0].ProductName);
        //    Assert.Equal("A", orderList9[0].CustomerCity);
        //    Assert.Equal("B", orderList9[1].ProductName);


        //    var orderList10 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhere(it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList10.Count);
        //    Assert.Equal("A", orderList10[0].ProductName);
        //    Assert.Equal("A", orderList10[0].CustomerCity);
        //    Assert.Equal("B", orderList10[1].ProductName);
        //    Assert.Equal("D", orderList10[2].ProductName);

        //    var orderList11 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .WhereIf(true, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList11.Count);
        //    Assert.Equal("A", orderList11[0].ProductName);
        //    Assert.Equal("A", orderList11[0].CustomerCity);
        //    Assert.Equal("B", orderList11[1].ProductName);
        //    Assert.Equal("D", orderList11[2].ProductName);

        //    var orderList12 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .WhereIf(false, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList12.Count);

        //    var orderList13 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhereIf(true, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList13.Count);
        //    Assert.Equal("A", orderList13[0].ProductName);
        //    Assert.Equal("A", orderList13[0].CustomerCity);
        //    Assert.Equal("B", orderList13[1].ProductName);
        //    Assert.Equal("D", orderList13[2].ProductName);

        //    var orderList14 = await orderHeaderRepository
        //        .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
        //        .OrderBy(it => it.T2.Quantity)
        //        .OrWhereIf(false, it => it.T3.CustomerNo == "A1")
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(3, orderList14.Count);
        //    Assert.Equal("A", orderList14[0].ProductName);
        //    Assert.Equal("A", orderList14[0].CustomerCity);
        //    Assert.Equal("B", orderList14[1].ProductName);
        //    Assert.Equal("D", orderList14[2].ProductName);

        //    #endregion
        //}


        /// <summary>
        /// 测试where条件中右边的值为dto的属性，并且同时测试从list里获取索引为0或者1的值
        /// </summary>
        [Fact, Priority(421)]
        public async Task TestParameterWithListPropertyDtoAndGetItem()
        {
            InitDatabase();

            var addressRepository = serviceProvider.GetService<IAddressRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();

            var customer1 = new Customer()
            {
                Age = 1,
                Name = "bob",
                CustomerNo = "A1",
                TotalConsumptionAmount = 1
            };

            var customer2 = new Customer()
            {
                Age = 2,
                Name = "jack",
                CustomerNo = "A2",
                TotalConsumptionAmount = 2
            };
            await customerRepository.InsertAsync(customer1);
            await customerRepository.InsertAsync(customer2);
            var baseTime = new DateTime(2023, 10, 24, 17, 0, 0);
            var date1 = baseTime.AddMinutes(-10);
            var date2 = baseTime.AddMinutes(-5);
            var date3 = baseTime.AddMinutes(-7);
            var date4 = baseTime.AddMinutes(-12);
            var address1 = new Address()
            {
                CustomerId = customer1.Id,
                City = "A",
                CreateOn = date3
            };
            var address2 = new Address()
            {
                CustomerId = customer2.Id,
                City = "B",
                CreateOn = date4
            };

            await addressRepository.InsertAsync(address1);
            await addressRepository.InsertAsync(address2);

            var p = new ParameterWithListPropertyDto()
            {
                DateTimes = new List<DateTime>() { date1, date2 }
            };

            var addressList = await addressRepository.Where(it => it.CreateOn > p.DateTimes[0] && it.CreateOn < p.DateTimes[1]).ToListAsync();

            Assert.Equal(1, addressList.Count);
            Assert.True(CompareTwoDate(date3, addressList[0].CreateOn));
            Assert.Equal("A", addressList[0].City);
        }

        private bool CompareTwoDate(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day &&
                   date1.Hour == date2.Hour && date1.Minute == date2.Minute && date1.Second == date2.Second;
        }

        ///// <summary>
        ///// 测试3张表联查
        ///// </summary>
        //[Fact, Priority(420)]
        //public async Task TestLeftJoin4TableAsync()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var customerRepository = serviceProvider.GetService<ICustomerRepository>();
        //    var addressRepository = serviceProvider.GetService<IAddressRepository>();
        //    var customer1 = new Customer()
        //    {
        //        Age = 1,
        //        Name = "bob",
        //        CustomerNo = "A1",
        //        TotalConsumptionAmount = 1
        //    };

        //    var customer2 = new Customer()
        //    {
        //        Age = 2,
        //        Name = "jack",
        //        CustomerNo = "A2",
        //        TotalConsumptionAmount = 2
        //    };
        //    await customerRepository.InsertAsync(customer1);
        //    await customerRepository.InsertAsync(customer2);
        //    var address1 = new Address()
        //    {
        //        CustomerId = customer1.Id,
        //        City = "A"
        //    };
        //    var address2 = new Address()
        //    {
        //        CustomerId = customer2.Id,
        //        City = "B"
        //    };
        //    await addressRepository.InsertAsync(address1);
        //    await addressRepository.InsertAsync(address2);

        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer1.Id,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer2.Id,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal("A", orderList[0].CustomerCity);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal("A", orderList[1].CustomerCity);
        //    Assert.Equal(1, orderList[1].Age);

        //    //test anonymous type
        //    var orderList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
        //        .ToListAsync();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal("A", orderList2[0].CustomerCity);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);
        //    Assert.Equal("A", orderList2[1].CustomerCity);


        //    var orderAddresses = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T4)
        //        .ToListAsync();
        //    Assert.Equal(2, orderAddresses.Count);
        //    Assert.Equal("A", orderAddresses[1].City);

        //    ////测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToPageAsync(pageable);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal("A", orderList[0].CustomerCity);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);
        //    Assert.Equal("A", orderList[1].CustomerCity);

        //    ////test anonymous type
        //    var orderPageList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
        //        .ToPageAsync(pageable);
        //    var orderList3 = orderPageList2.Data;
        //    Assert.Equal(2, orderList3.Count);
        //    Assert.Equal(1, orderList3[0].Quantity);
        //    Assert.Equal("A", orderList3[0].ProductName);
        //    Assert.Equal("ABC", orderList3[0].OrderNo);
        //    Assert.Equal("A1", orderList3[0].CustomerNo);
        //    Assert.Equal(1, orderList3[0].Age);
        //    Assert.Equal("A", orderList3[0].CustomerCity);
        //    Assert.Equal(2, orderList3[1].Quantity);
        //    Assert.Equal("B", orderList3[1].ProductName);
        //    Assert.Equal("ABC", orderList3[1].OrderNo);
        //    Assert.Equal("A1", orderList3[1].CustomerNo);
        //    Assert.Equal(1, orderList3[1].Age);
        //    Assert.Equal("A", orderList3[1].CustomerCity);

        //    var orderAddressPages = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T4)
        //        .ToPageAsync(pageable);

        //    orderAddresses = orderAddressPages.Data;
        //    Assert.Equal(2, orderAddresses.Count);
        //    Assert.Equal("A", orderAddresses[1].City);
        //}

        ///// <summary>
        ///// 测试3张表联查
        ///// </summary>
        //[Fact, Priority(419)]
        //public async Task TestLeftJoin3TableAsync()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var customerRepository = serviceProvider.GetService<ICustomerRepository>();
        //    var customer1 = new Customer()
        //    {
        //        Age = 1,
        //        Name = "bob",
        //        CustomerNo = "A1",
        //        TotalConsumptionAmount = 1
        //    };

        //    var customer2 = new Customer()
        //    {
        //        Age = 2,
        //        Name = "jack",
        //        CustomerNo = "A2",
        //        TotalConsumptionAmount = 2
        //    };
        //    await customerRepository.InsertAsync(customer1);
        //    await customerRepository.InsertAsync(customer2);

        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer1.Id,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer2.Id,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);

        //    //test anonymous type
        //    var orderList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
        //        .ToListAsync();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);


        //    var orderCustomers = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T3)
        //        .ToListAsync();
        //    Assert.Equal(2, orderCustomers.Count);
        //    Assert.Equal("A1", orderCustomers[1].CustomerNo);
        //    Assert.Equal("bob", orderCustomers[1].Name);

        //    ////测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToPageAsync(pageable);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);

        //    ////test anonymous type
        //    var orderPageList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
        //        .ToPageAsync(pageable);
        //    orderList2 = orderPageList2.Data;
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);

        //    var orderCustomerPages = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T3)
        //        .ToPageAsync(pageable);
        //    orderCustomers = orderCustomerPages.Data;
        //    Assert.Equal(2, orderCustomers.Count);
        //    Assert.Equal("A1", orderCustomers[1].CustomerNo);
        //    Assert.Equal("bob", orderCustomers[1].Name);
        //}
        ///// <summary>
        ///// 测试2张表联查
        ///// </summary>
        //[Fact, Priority(418)]
        //public async Task TestLeftJoin2TableAsync()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = 1,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = 2,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
        //        .ToListAsync();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal(1, orderList[1].CustomerId);

        //    //test anonymous type
        //    var orderList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
        //        .ToListAsync();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal(1, orderList2[1].CustomerId);


        //    var orderDetails = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderByDescending(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T2)
        //        .ToListAsync();
        //    Assert.Equal(2, orderDetails.Count);
        //    Assert.Equal(1, orderDetails[1].Quantity);
        //    Assert.Equal("A", orderDetails[1].ProductName);
        //    Assert.Equal(2, orderDetails[0].Quantity);
        //    Assert.Equal("B", orderDetails[0].ProductName);

        //    //测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
        //        .ToPageAsync(pageable);

        //    Assert.Equal(2, orderPageList.TotalPages);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal(1, orderList[1].CustomerId);

        //    //test anonymous type
        //    var orderPageList2 = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
        //        .ToPageAsync(pageable);

        //    Assert.Equal(2, orderPageList2.TotalPages);
        //    orderList2 = orderPageList2.Data;
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal(1, orderList2[1].CustomerId);

        //    var orderPageDetails = await orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderByDescending(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T2)
        //        .ToPageAsync(pageable);
        //    Assert.Equal(2, orderPageDetails.TotalPages);
        //    orderDetails = orderPageDetails.Data;
        //    Assert.Equal(2, orderDetails.Count);
        //    Assert.Equal(1, orderDetails[1].Quantity);
        //    Assert.Equal("A", orderDetails[1].ProductName);
        //    Assert.Equal(2, orderDetails[0].Quantity);
        //    Assert.Equal("B", orderDetails[0].ProductName);
        //}

        ///// <summary>
        ///// 测试3张表联查
        ///// </summary>
        //[Fact, Priority(417)]
        //public async Task TestLeftJoin4Table()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var customerRepository = serviceProvider.GetService<ICustomerRepository>();
        //    var addressRepository = serviceProvider.GetService<IAddressRepository>();
        //    var customer1 = new Customer()
        //    {
        //        Age = 1,
        //        Name = "bob",
        //        CustomerNo = "A1",
        //        TotalConsumptionAmount = 1
        //    };

        //    var customer2 = new Customer()
        //    {
        //        Age = 2,
        //        Name = "jack",
        //        CustomerNo = "A2",
        //        TotalConsumptionAmount = 2
        //    };
        //    await customerRepository.InsertAsync(customer1);
        //    await customerRepository.InsertAsync(customer2);
        //    var address1 = new Address()
        //    {
        //        CustomerId = customer1.Id,
        //        City = "A"
        //    };
        //    var address2 = new Address()
        //    {
        //        CustomerId = customer2.Id,
        //        City = "B"
        //    };
        //    await addressRepository.InsertAsync(address1);
        //    await addressRepository.InsertAsync(address2);

        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer1.Id,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer2.Id,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToList();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal("A", orderList[0].CustomerCity);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal("A", orderList[1].CustomerCity);
        //    Assert.Equal(1, orderList[1].Age);

        //    //test anonymous type
        //    var orderList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
        //        .ToList();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal("A", orderList2[0].CustomerCity);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);
        //    Assert.Equal("A", orderList2[1].CustomerCity);


        //    var orderAddresses = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T4)
        //        .ToList();
        //    Assert.Equal(2, orderAddresses.Count);
        //    Assert.Equal("A", orderAddresses[1].City);

        //    ////测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
        //        .ToPage(pageable);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal("A", orderList[0].CustomerCity);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);
        //    Assert.Equal("A", orderList[1].CustomerCity);

        //    ////test anonymous type
        //    var orderPageList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
        //        .ToPage(pageable);
        //    var orderList3 = orderPageList2.Data;
        //    Assert.Equal(2, orderList3.Count);
        //    Assert.Equal(1, orderList3[0].Quantity);
        //    Assert.Equal("A", orderList3[0].ProductName);
        //    Assert.Equal("ABC", orderList3[0].OrderNo);
        //    Assert.Equal("A1", orderList3[0].CustomerNo);
        //    Assert.Equal(1, orderList3[0].Age);
        //    Assert.Equal("A", orderList3[0].CustomerCity);
        //    Assert.Equal(2, orderList3[1].Quantity);
        //    Assert.Equal("B", orderList3[1].ProductName);
        //    Assert.Equal("ABC", orderList3[1].OrderNo);
        //    Assert.Equal("A1", orderList3[1].CustomerNo);
        //    Assert.Equal(1, orderList3[1].Age);
        //    Assert.Equal("A", orderList3[1].CustomerCity);

        //    var orderAddressPages = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T4)
        //        .ToPage(pageable);

        //    orderAddresses = orderAddressPages.Data;
        //    Assert.Equal(2, orderAddresses.Count);
        //    Assert.Equal("A", orderAddresses[1].City);
        //}

        ///// <summary>
        ///// 测试3张表联查
        ///// </summary>
        //[Fact, Priority(416)]
        //public async Task TestLeftJoin3Table()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var customerRepository = serviceProvider.GetService<ICustomerRepository>();
        //    var customer1 = new Customer()
        //    {
        //        Age = 1,
        //        Name = "bob",
        //        CustomerNo = "A1",
        //        TotalConsumptionAmount = 1
        //    };

        //    var customer2 = new Customer()
        //    {
        //        Age = 2,
        //        Name = "jack",
        //        CustomerNo = "A2",
        //        TotalConsumptionAmount = 2
        //    };
        //    await customerRepository.InsertAsync(customer1);
        //    await customerRepository.InsertAsync(customer2);

        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer1.Id,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = customer2.Id,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToList();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);

        //    //test anonymous type
        //    var orderList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
        //        .ToList();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);


        //    var orderCustomers = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T3)
        //        .ToList();
        //    Assert.Equal(2, orderCustomers.Count);
        //    Assert.Equal("A1", orderCustomers[1].CustomerNo);
        //    Assert.Equal("bob", orderCustomers[1].Name);

        //    ////测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
        //        .ToPage(pageable);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal("A1", orderList[0].CustomerNo);
        //    Assert.Equal(1, orderList[0].Age);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal("A1", orderList[1].CustomerNo);
        //    Assert.Equal(1, orderList[1].Age);

        //    ////test anonymous type
        //    var orderPageList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
        //        .ToPage(pageable);
        //    orderList2 = orderPageList2.Data;
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal("A1", orderList2[0].CustomerNo);
        //    Assert.Equal(1, orderList2[0].Age);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal("A1", orderList2[1].CustomerNo);
        //    Assert.Equal(1, orderList2[1].Age);

        //    var orderCustomerPages = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
        //        .OrderByDescending(it => it.T3.Age)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T3)
        //        .ToPage(pageable);
        //    orderCustomers = orderCustomerPages.Data;
        //    Assert.Equal(2, orderCustomers.Count);
        //    Assert.Equal("A1", orderCustomers[1].CustomerNo);
        //    Assert.Equal("bob", orderCustomers[1].Name);
        //}
        ///// <summary>
        ///// 测试2张表联查
        ///// </summary>
        //[Fact, Priority(415)]
        //public async Task TestLeftJoin2Table()
        //{
        //    InitDatabase();
        //    var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
        //    var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
        //    var orderHeader = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = 1,
        //        OrderNo = "ABC",
        //        State = 1
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader);

        //    var orderHeader2 = new OrderHeader()
        //    {
        //        CreateTime = DateTime.Now,
        //        CustomerId = 2,
        //        OrderNo = "DEF",
        //        State = 2
        //    };
        //    await orderHeaderRepository.InsertAsync(orderHeader2);
        //    var orderDetail1 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "A",
        //        Quantity = 1,
        //        State = 1
        //    };

        //    var orderDetail2 = new OrderDetail()
        //    {
        //        OrderHeaderId = orderHeader.Id,
        //        ProductName = "B",
        //        Quantity = 2,
        //        State = 1
        //    };
        //    await orderDetailRepository.InsertAsync(orderDetail1);
        //    await orderDetailRepository.InsertAsync(orderDetail2);

        //    var orderList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
        //        .ToList();
        //    Assert.Equal(2, orderList.Count);
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal(1, orderList[1].CustomerId);

        //    //test anonymous type
        //    var orderList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
        //        .ToList();
        //    Assert.Equal(2, orderList2.Count);
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal(1, orderList2[1].CustomerId);


        //    var orderDetails = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderByDescending(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T2)
        //        .ToList();
        //    Assert.Equal(2, orderDetails.Count);
        //    Assert.Equal(1, orderDetails[1].Quantity);
        //    Assert.Equal("A", orderDetails[1].ProductName);
        //    Assert.Equal(2, orderDetails[0].Quantity);
        //    Assert.Equal("B", orderDetails[0].ProductName);

        //    //测试分页
        //    var pageable = new Pageable(1, 5);
        //    var orderPageList = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
        //        .ToPage(pageable);

        //    Assert.Equal(2, orderPageList.TotalPages);
        //    orderList = orderPageList.Data;
        //    Assert.Equal(1, orderList[0].Quantity);
        //    Assert.Equal("A", orderList[0].ProductName);
        //    Assert.Equal("ABC", orderList[0].OrderNo);
        //    Assert.Equal(2, orderList[1].Quantity);
        //    Assert.Equal("B", orderList[1].ProductName);
        //    Assert.Equal("ABC", orderList[1].OrderNo);
        //    Assert.Equal(1, orderList[1].CustomerId);

        //    //test anonymous type
        //    var orderPageList2 = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderBy(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
        //        .ToPage(pageable);

        //    Assert.Equal(2, orderPageList2.TotalPages);
        //    orderList2 = orderPageList2.Data;
        //    Assert.Equal(1, orderList2[0].Quantity);
        //    Assert.Equal("A", orderList2[0].ProductName);
        //    Assert.Equal("ABC", orderList2[0].OrderNo);
        //    Assert.Equal(2, orderList2[1].Quantity);
        //    Assert.Equal("B", orderList2[1].ProductName);
        //    Assert.Equal("ABC", orderList2[1].OrderNo);
        //    Assert.Equal(1, orderList2[1].CustomerId);

        //    var orderPageDetails = orderHeaderRepository
        //        .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
        //        .OrderByDescending(it => it.T2.Quantity)
        //        .Where(it => it.T1.State == 1)
        //        .Select(it => it.T2)
        //        .ToPage(pageable);
        //    Assert.Equal(2, orderPageDetails.TotalPages);
        //    orderDetails = orderPageDetails.Data;
        //    Assert.Equal(2, orderDetails.Count);
        //    Assert.Equal(1, orderDetails[1].Quantity);
        //    Assert.Equal("A", orderDetails[1].ProductName);
        //    Assert.Equal(2, orderDetails[0].Quantity);
        //    Assert.Equal("B", orderDetails[0].ProductName);
        //}

        /// <summary>
        /// 测试插入实体和更新实体前的自定义函数
        /// </summary>
        [Fact, Priority(414)]
        public async Task TestBeforeInsertAndUpdateEvent()
        {
            InitDatabase();
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
            var id = Guid.NewGuid();
            var guidModel = new GuidModel()
            {
                Id = id,
                Name = "sb"
            };
            await guidModelRepository.InsertAsync(guidModel);
            Assert.Equal("abc", guidModel.Address);
            guidModel.Name = "ccd";
            await guidModelRepository.UpdateAsync(guidModel);
            Assert.Equal("ppp", guidModel.Address);

            id = Guid.NewGuid();
            var guidModel2 = new GuidModel()
            {
                Id = id,
                Name = "sb"
            };
            guidModelRepository.Insert(guidModel2);
            Assert.Equal("abc", guidModel2.Address);
            guidModel2.Name = "ccd";
            guidModelRepository.UpdateAsync(guidModel2);
            Assert.Equal("ppp", guidModel2.Address);
        }

        /// <summary>
        /// 测试id类型为guid的model的增删改查
        /// </summary>
        [Fact, Priority(413)]
        public async Task TestModelUseGuidAsId()
        {
            InitDatabase();
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();

            var id = Guid.NewGuid();
            var guidModel = new GuidModel()
            {
                Id = id,
                Name = "sb"
            };
            await guidModelRepository.InsertAsync(guidModel);
            var dbGuidModel = await guidModelRepository.GetAsync(id);
            Assert.Equal(guidModel, dbGuidModel);
            var dbGuidModel2 = guidModelRepository.FirstOrDefault(it => it.Id == id);
            Assert.Equal(guidModel, dbGuidModel2);
            dbGuidModel2.Name = "sb2";
            await guidModelRepository.UpdateAsync(dbGuidModel2);
            var dbGuidModel3 = guidModelRepository.Where(it => it.Name == "sb2").ToList();
            Assert.Equal(id, dbGuidModel3.FirstOrDefault()?.Id);
            await guidModelRepository.DeleteAsync(dbGuidModel3.FirstOrDefault());
            var nullDbGuidModel = await guidModelRepository.GetAsync(id);
            Assert.Null(nullDbGuidModel);
        }

        /// <summary>
        /// 测试事务中批量插入
        /// </summary>
        [Fact, Priority(412)]
        public async Task TestBatchInsertWithDbtransation()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now,
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
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);

            unitOfWork.BeginTransaction();
            try
            {
                await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
                throw new Exception("error");
                unitOfWork.Commit();
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }

            var count1 = (await nullableTableRepository.GetAllAsync()).Count;
            Assert.Equal(2000, count1);
            unitOfWork.BeginTransaction();
            try
            {
                await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
                unitOfWork.Commit();
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }
            var count2 = (await nullableTableRepository.GetAllAsync()).Count;
            Assert.Equal(4000, count2);
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(411)]
        public async Task TestBatchInsertAsync()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IDbFactory>();
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now,
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
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            sw.Start();
            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
            var nullableTableList2 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now2,
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
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList2.Add(a);
            }
            sw.Restart();
            await nullableTableRepository.InsertAsync(nullableTableList2);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;
            var nullableTableList3 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now,
                    Decimal2 = 1m,
                    Decimal3 = 1.1m,
                    Double2 = 1.1,
                    Float2 = (float)1.1,
                    Guid2 = Guid.NewGuid(),
                    Id = 0,
                    Short2 = 1,
                    TimeSpan2 = TimeSpan.FromHours(1),
                    String2 = "sb",
                    String3 = "sb",
                    Long2 = 2,
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList3.Add(a);
            }
            sw.Restart();
            using (var dbConnection = new SqlConnection(connectionString))
            {
                //&SqlBulkCopyOptions.KeepNulls
                dbConnection.Open();
                var dbtran = dbConnection.BeginTransaction();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity,
                    dbtran);

                sqlBulkCopy.BatchSize = total;
                sqlBulkCopy.DestinationTableName = "NullableTable";
                sqlBulkCopy.ColumnMappings.Add("Int2", "Int2");
                sqlBulkCopy.ColumnMappings.Add("Bool2", "Bool2");
                sqlBulkCopy.ColumnMappings.Add("Byte2", "Byte2");
                sqlBulkCopy.ColumnMappings.Add("DateTime2", "DateTime2");
                sqlBulkCopy.ColumnMappings.Add("Decimal2", "Decimal2");
                sqlBulkCopy.ColumnMappings.Add("Decimal3", "Decimal3");
                sqlBulkCopy.ColumnMappings.Add("Double2", "Double2");
                sqlBulkCopy.ColumnMappings.Add("Float2", "Float2");
                sqlBulkCopy.ColumnMappings.Add("Guid2", "Guid2");
                sqlBulkCopy.ColumnMappings.Add("Short2", "Short2");
                sqlBulkCopy.ColumnMappings.Add("TimeSpan2", "TimeSpan2");

                sqlBulkCopy.ColumnMappings.Add("String2", "String2");
                sqlBulkCopy.ColumnMappings.Add("String3", "String3");
                sqlBulkCopy.ColumnMappings.Add("Long2", "Long2");
                sqlBulkCopy.ColumnMappings.Add("Enum2", "Enum2");
                sqlBulkCopy.ColumnMappings.Add("Int3", "TestInt3");

                var table = nullableTableList3.ToDataTable();
                //sqlserver替换timespan类型为long类型
                //SbUtil.ReplaceDataTableTimeSpanColumnForSqlserver(table);
                //table.Columns.Remove("id");
                await sqlBulkCopy.WriteToServerAsync(table);
                dbtran.Commit();
            }
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
            var models = nullableTableRepository.Where(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id))
                .ToList();
            var count = nullableTableRepository.Count(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id));
            Assert.Equal(3, count);
            Assert.Equal(3, models.Count);
            Assert.True(models[0].Equals(models[1]));
            Assert.True(models[0].Equals(models[2]));
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(410)]
        public async Task TestBatchInsert()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();

            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now,
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
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            sw.Start();
            nullableTableRepository.FastBatchInsert(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
            var nullableTableList2 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now2,
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
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList2.Add(a);
            }
            sw.Restart();
            nullableTableRepository.Insert(nullableTableList2);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;
            var nullableTableList3 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now,
                    Decimal2 = 1m,
                    Decimal3 = 1.1m,
                    Double2 = 1.1,
                    Float2 = (float)1.1,
                    Guid2 = Guid.NewGuid(),
                    Id = 0,
                    Short2 = 1,
                    TimeSpan2 = TimeSpan.FromHours(1),
                    String2 = "sb",
                    String3 = "sb",
                    Long2 = 2,
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList3.Add(a);
            }
            sw.Restart();
            using (var dbConnection = new SqlConnection(connectionString))
            {
                //&SqlBulkCopyOptions.KeepNulls
                dbConnection.Open();
                var dbtran = dbConnection.BeginTransaction();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity,
                    dbtran);
                sqlBulkCopy.BatchSize = total;
                sqlBulkCopy.DestinationTableName = "NullableTable";
                sqlBulkCopy.ColumnMappings.Add("Int2", "Int2");
                sqlBulkCopy.ColumnMappings.Add("Bool2", "Bool2");
                sqlBulkCopy.ColumnMappings.Add("Byte2", "Byte2");
                sqlBulkCopy.ColumnMappings.Add("DateTime2", "DateTime2");
                sqlBulkCopy.ColumnMappings.Add("Decimal2", "Decimal2");
                sqlBulkCopy.ColumnMappings.Add("Decimal3", "Decimal3");
                sqlBulkCopy.ColumnMappings.Add("Double2", "Double2");
                sqlBulkCopy.ColumnMappings.Add("Float2", "Float2");
                sqlBulkCopy.ColumnMappings.Add("Guid2", "Guid2");
                sqlBulkCopy.ColumnMappings.Add("Short2", "Short2");
                sqlBulkCopy.ColumnMappings.Add("TimeSpan2", "TimeSpan2");

                sqlBulkCopy.ColumnMappings.Add("String2", "String2");
                sqlBulkCopy.ColumnMappings.Add("String3", "String3");
                sqlBulkCopy.ColumnMappings.Add("Long2", "Long2");
                sqlBulkCopy.ColumnMappings.Add("Enum2", "Enum2");
                sqlBulkCopy.ColumnMappings.Add("Int3", "TestInt3");

                var table = nullableTableList3.ToDataTable();
                //sqlserver替换timespan类型为long类型
                //SbUtil.ReplaceDataTableTimeSpanColumnForSqlserver(table);
                //table.Columns.Remove("id");
                sqlBulkCopy.WriteToServer(table);
                dbtran.Commit();
            }
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
            var models = nullableTableRepository.Where(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id))
                .ToList();
            var count = nullableTableRepository.Count(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id));
            Assert.Equal(3, count);
            Assert.Equal(3, models.Count);
            Assert.True(models[0].Equals(models[1]));
            Assert.True(models[0].Equals(models[2]));
        }

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(409)]
        public async Task TestGetSqlByConfigurationAsync()
        {
            InitDatabase();
            var testConfigurationRepository = serviceProvider.GetService<ICustomerTestConfigurationRepository>();
            var customer1 = new Customer() { Age = 3, Name = "sb" };
            var customer2 = new Customer() { Age = 5, Name = "sb2" };
            testConfigurationRepository.Insert(customer1);
            var customerList = await testConfigurationRepository.QueryListAsync();
            Assert.Equal(1, customerList.Count);
            Assert.Equal("sb", customerList[0].Name);

            testConfigurationRepository.Insert(customer2);
            var customerList2 = await testConfigurationRepository.QueryByPageAsync(new Pageable(1, 10));
            Assert.Equal(2, customerList2.Data.Count);
            Assert.Equal("sb", customerList2.Data[0].Name);
            Assert.Equal("sb2", customerList2.Data[1].Name);

            await testConfigurationRepository.UpdateByNameAsync("sb", 7);
            var dbCustomer = testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Equal(7, dbCustomer.Age);
            await testConfigurationRepository.DeleteByNameAsync("sb");
            var dbCustomer2 = testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Null(dbCustomer2);
        }

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(408)]
        public void TestGetSqlByConfiguration()
        {
            InitDatabase();
            var testConfigurationRepository = serviceProvider.GetService<ICustomerTestConfigurationRepository>();
            var customer1 = new Customer() { Age = 3, Name = "sb" };
            var customer2 = new Customer() { Age = 5, Name = "sb2" };
            testConfigurationRepository.Insert(customer1);
            var customerList = testConfigurationRepository.QueryList();
            Assert.Equal(1, customerList.Count);
            Assert.Equal("sb", customerList[0].Name);

            testConfigurationRepository.Insert(customer2);
            var customerList2 = testConfigurationRepository.QueryByPage(new Pageable(1, 10));
            Assert.Equal(2, customerList2.Data.Count);
            Assert.Equal("sb", customerList2.Data[0].Name);
            Assert.Equal("sb2", customerList2.Data[1].Name);

            testConfigurationRepository.UpdateByName("sb", 7);
            var dbCustomer = testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Equal(7, dbCustomer.Age);
            testConfigurationRepository.DeleteByName("sb");
            var dbCustomer2 = testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Null(dbCustomer2);
        }

        /// <summary>
        /// 测试带命名空间的情况和新增主键
        /// </summary>
        [Fact, Priority(407)]
        public void TestTableSchemaAndAddPrimaryKey()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var customerWithSchema2Repository = serviceProvider.GetService<ICustomerWithSchema2Repository>();
            var sb = new StringBuilder();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema) });
            sb.Clear();
            sb.AppendLine("CREATE TABLE test1.[CustomerWithSchema] (");
            sb.AppendLine("    [Name] nvarchar(max) NULL,");
            sb.AppendLine("    [Age] int NOT NULL,");
            sb.AppendLine("    [CustomerNo] nvarchar(max) NULL,");
            sb.AppendLine("    [TotalConsumptionAmount] decimal(18,2) NOT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema2) });
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [Id] int IDENTITY(1,1) PRIMARY KEY NOT NULL", result[0].FieldModifySqls[0]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [LastUpdateOn] datetime2 NULL", result[0].FieldModifySqls[1]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [LastUpdateBy] nvarchar(max) NULL", result[0].FieldModifySqls[2]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [CreateOn] datetime2 NULL", result[0].FieldModifySqls[3]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [CreateBy] nvarchar(max) NULL", result[0].FieldModifySqls[4]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [Active] int NULL", result[0].FieldModifySqls[5]);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }

            var entity = new CustomerWithSchema2()
            {
                Name = "sb",
                Age = 3
            };
            customerWithSchema2Repository.Insert(entity);

            var customerWithSchema2 = customerWithSchema2Repository.FirstOrDefault(it => it.Name == "sb");
            Assert.NotNull(customerWithSchema2);
            Assert.Equal("sb", customerWithSchema2.Name);
            customerWithSchema2.Name = "sb3";
            customerWithSchema2Repository.Update(customerWithSchema2);
            var customerWithSchema3 = customerWithSchema2Repository.FirstOrDefault(it => it.Name == "sb3");
            Assert.NotNull(customerWithSchema3);
            Assert.Equal("sb3", customerWithSchema3.Name);

            var entity2 = new CustomerWithSchema2()
            {
                Name = "sb4",
                Age = 5
            };
            customerWithSchema2Repository.Insert(entity2);
            var customerWithSchemaPage = customerWithSchema2Repository.TestPage(new Pageable(1, 10));
            Assert.Equal(2, customerWithSchemaPage.Data.Count);
            Assert.Equal("sb3", customerWithSchemaPage.Data[0].Name);
            Assert.Equal("sb4", customerWithSchemaPage.Data[1].Name);
            Assert.Equal(3, customerWithSchemaPage.Data[0].Age);
            Assert.Equal(5, customerWithSchemaPage.Data[1].Age);

            customerWithSchema2Repository.Delete(it => it.Name == "sb3");
            var customerWithSchema4 = customerWithSchema2Repository.FirstOrDefault(it => it.Name == "sb3");
            Assert.Null(customerWithSchema4);
        }


        /// <summary>
        /// 测试根据实体类创建数据库表和进行插入查询对照
        /// </summary>
        [Fact, Priority(406)]
        public void TestCreateTableFromEntityAndCrud()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var nullableTable2Repository = serviceProvider.GetService<INullableTable2Repository>();
            var sqls = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2) });
            foreach (var sql in sqls)
            {
                dbGenerator.ExecuteGenerateSql(sql);
            }

            var now = DateTime.Now;
            var entity = new NullableTable2()
            {
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = now,
                Decimal2 = 1,
                Decimal3 = 1,
                Double2 = 1,
                Float2 = 1,
                Guid2 = Guid.NewGuid(),
                Int2 = 1,
                Long2 = 1,
                Short2 = 1,
                String2 = "sb",
                String3 = "sb",
                TimeSpan2 = TimeSpan.FromHours(1)
            };
            nullableTable2Repository.Insert(entity);

            var dbEntity = nullableTable2Repository.FirstOrDefault(it => it.String2 == "sb");

            CompareTwoNullable(entity, dbEntity);

            var entity2 = new NullableTable2()
            {
                Bool2 = null,
                Byte2 = null,
                DateTime2 = null,
                Decimal2 = null,
                Decimal3 = null,
                Double2 = null,
                Float2 = null,
                Guid2 = null,
                Int2 = null,
                Long2 = null,
                Short2 = null,
                String2 = "sb2",
                String3 = null,
                TimeSpan2 = null
            };
            nullableTable2Repository.Insert(entity2);

            var dbEntity2 = nullableTable2Repository.FirstOrDefault(it => it.String2 == "sb2");

            CompareTwoNullable(entity2, dbEntity2);
        }

        private void CompareTwoNullable(NullableTable2 entity, NullableTable2 dbEntity)
        {
            Assert.NotNull(dbEntity);
            Assert.Equal(entity.Bool2, dbEntity.Bool2);
            Assert.Equal(entity.Byte2, dbEntity.Byte2);
            Assert.True((dbEntity.DateTime2.GetValueOrDefault() - entity.DateTime2.GetValueOrDefault()).TotalSeconds < 2);
            Assert.Equal(entity.Decimal2, dbEntity.Decimal2);
            Assert.Equal(entity.Decimal3, dbEntity.Decimal3);
            Assert.Equal(entity.Double2, dbEntity.Double2);
            Assert.Equal(entity.Guid2, dbEntity.Guid2);
            Assert.Equal(entity.Int2, dbEntity.Int2);
            Assert.Equal(entity.Long2, dbEntity.Long2);
            Assert.Equal(entity.Short2, dbEntity.Short2);
            Assert.Equal(entity.String2, dbEntity.String2);
            Assert.Equal(entity.String3, dbEntity.String3);
            Assert.Equal(entity.TimeSpan2, dbEntity.TimeSpan2);
        }


        /// <summary>
        /// 测试表名字段名映射
        /// </summary>
        [Fact, Priority(405)]
        public void TestTableColumnMap()
        {
            InitDatabase();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var tableColumnMapRepository = serviceProvider.GetService<ITableColumnMapRepository>();
            customerRepository.Insert(new Customer() { Name = "sb" });
            var customer = tableColumnMapRepository.FirstOrDefault(it => it.CustomerName == "sb");
            Assert.NotNull(customer);
            Assert.Equal("sb", customer.CustomerName);
        }
        [Fact, Priority(404)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateCsharpClass(new List<string>() { "Customer", "NullableTable", "NotNullableTable" }, "abc");
            Assert.Equal(3, result.Count);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("namespace abc");
            sb.AppendLine("{");
            sb.AppendLine("   [Table(\"Customer\")]");
            sb.AppendLine("   public class Customer");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"Id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      [Column(\"Name\")]");
            sb.AppendLine("      public string Name { get; set; }");
            sb.AppendLine("      [Column(\"Age\")]");
            sb.AppendLine("      public int Age { get; set; }");
            sb.AppendLine("      [Column(\"CustomerNo\")]");
            sb.AppendLine("      public string CustomerNo { get; set; }");
            sb.AppendLine("      [Column(\"TotalConsumptionAmount\")]");
            sb.AppendLine("      public decimal TotalConsumptionAmount { get; set; }");
            sb.AppendLine("      [Column(\"BirthDay\")]");
            sb.AppendLine("      public DateTime? BirthDay { get; set; }");
            sb.AppendLine("   }");
            sb.AppendLine("}");
            var exceptStr = sb.ToString();

            Assert.Equal(exceptStr
                , result[0]);

            sb.Clear();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("namespace abc");
            sb.AppendLine("{");
            sb.AppendLine("   /// <summary>");
            sb.AppendLine("   ///NullableTable");
            sb.AppendLine("   /// </summary>");
            sb.AppendLine("   [Table(\"NullableTable\")]");
            sb.AppendLine("   public class NullableTable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"Id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"Int2\")]");
            sb.AppendLine("      public int? Int2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"Long2\")]");
            sb.AppendLine("      public long? Long2 { get; set; }");
            sb.AppendLine("      [Column(\"Float2\")]");
            sb.AppendLine("      public float? Float2 { get; set; }");
            sb.AppendLine("      [Column(\"Double2\")]");
            sb.AppendLine("      public double? Double2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal2\")]");
            sb.AppendLine("      public decimal? Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal3\")]");
            sb.AppendLine("      public decimal? Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"Guid2\")]");
            sb.AppendLine("      public Guid? Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"Short2\")]");
            sb.AppendLine("      public short? Short2 { get; set; }");
            sb.AppendLine("      [Column(\"DateTime2\")]");
            sb.AppendLine("      public DateTime? DateTime2 { get; set; }");
            sb.AppendLine("      [Column(\"Bool2\")]");
            sb.AppendLine("      public bool? Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"TimeSpan2\")]");
            sb.AppendLine("      public TimeSpan? TimeSpan2 { get; set; }");
            sb.AppendLine("      [Column(\"Byte2\")]");
            sb.AppendLine("      public byte? Byte2 { get; set; }");
            sb.AppendLine("      [Column(\"String2\")]");
            sb.AppendLine("      public string String2 { get; set; }");
            sb.AppendLine("      [Column(\"String3\")]");
            sb.AppendLine("      public string String3 { get; set; }");
            sb.AppendLine("      [Column(\"Enum2\")]");
            sb.AppendLine("      public int? Enum2 { get; set; }");
            sb.AppendLine("      [Column(\"TestInt3\")]");
            sb.AppendLine("      public int? TestInt3 { get; set; }");
            sb.AppendLine("   }");
            sb.AppendLine("}");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1]);

            sb.Clear();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("namespace abc");
            sb.AppendLine("{");
            sb.AppendLine("   /// <summary>");
            sb.AppendLine("   ///NotNullableTable");
            sb.AppendLine("   /// </summary>");
            sb.AppendLine("   [Table(\"NotNullableTable\")]");
            sb.AppendLine("   public class NotNullableTable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"Id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"Int2\")]");
            sb.AppendLine("      public int Int2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"Long2\")]");
            sb.AppendLine("      public long Long2 { get; set; }");
            sb.AppendLine("      [Column(\"Float2\")]");
            sb.AppendLine("      public float Float2 { get; set; }");
            sb.AppendLine("      [Column(\"Double2\")]");
            sb.AppendLine("      public double Double2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal2\")]");
            sb.AppendLine("      public decimal Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal3\")]");
            sb.AppendLine("      public decimal Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"Guid2\")]");
            sb.AppendLine("      public Guid Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"Short2\")]");
            sb.AppendLine("      public short Short2 { get; set; }");
            sb.AppendLine("      [Column(\"DateTime2\")]");
            sb.AppendLine("      public DateTime DateTime2 { get; set; }");
            sb.AppendLine("      [Column(\"Bool2\")]");
            sb.AppendLine("      public bool Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"TimeSpan2\")]");
            sb.AppendLine("      public TimeSpan TimeSpan2 { get; set; }");
            sb.AppendLine("      [Column(\"Byte2\")]");
            sb.AppendLine("      public byte Byte2 { get; set; }");
            sb.AppendLine("      [Column(\"String2\")]");
            sb.AppendLine("      public string String2 { get; set; }");
            sb.AppendLine("      [Column(\"String3\")]");
            sb.AppendLine("      public string String3 { get; set; }");
            sb.AppendLine("   }");
            sb.AppendLine("}");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[2]);
        }

        /// <summary>
        /// 测试根据c#类生成数据库表
        /// </summary>
        [Fact, Priority(403)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {

            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE dbo.[NullableTable2] (");
            sb.AppendLine("    [Id] int IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("    [Int2] int NULL,");
            sb.AppendLine("    [Long2] bigint NULL,");
            sb.AppendLine("    [Float2] real NULL,");
            sb.AppendLine("    [Double2] float NULL,");
            sb.AppendLine("    [Decimal2] decimal(18,2) NULL,");
            sb.AppendLine("    [Decimal3] decimal(20,4) NULL,");
            sb.AppendLine("    [Guid2] uniqueidentifier NULL,");
            sb.AppendLine("    [Short2] smallint NULL,");
            sb.AppendLine("    [DateTime2] datetime2 NULL,");
            sb.AppendLine("    [Bool2] bit NULL,");
            sb.AppendLine("    [TimeSpan2] time NULL,");
            sb.AppendLine("    [Byte2] tinyint NULL,");
            sb.AppendLine("    [String2] nvarchar(100) NULL,");
            sb.AppendLine("    [String3] nvarchar(max) NULL,");
            sb.AppendLine("    [Enum2] int NULL,");
            sb.AppendLine("    [TestInt3] int NULL,");
            sb.AppendLine("    CONSTRAINT PK_NullableTable2 PRIMARY KEY (Id)");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(4, result[0].Descriptions.Count);
            Assert.Equal("EXEC sp_addextendedproperty 'MS_Description', N'NullableTable2', 'schema', N'dbo', 'table', N'NullableTable2'", result[0].Descriptions[0]);
            Assert.Equal("EXEC sp_addextendedproperty 'MS_Description', N'Int2', 'schema', N'dbo', 'table', N'NullableTable2', 'column', N'Int2'", result[0].Descriptions[1]);
            Assert.Equal("EXEC sp_addextendedproperty 'MS_Description', N'Long2', 'schema', N'dbo', 'table', N'NullableTable2', 'column', N'Long2'", result[0].Descriptions[2]);
            Assert.Equal("EXEC sp_addextendedproperty 'MS_Description', N'Int2', 'schema', N'dbo', 'table', N'NullableTable2', 'column', N'TestInt3'", result[0].Descriptions[3]);
            //dbGenerator.ExecuteGenerateSql(result[0]);

            sb.Clear();
            sb.AppendLine("CREATE TABLE dbo.[NotNullableTable2] (");
            sb.AppendLine("    [Id] int IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("    [Int2] int NOT NULL,");
            sb.AppendLine("    [Long2] bigint NOT NULL,");
            sb.AppendLine("    [Float2] real NOT NULL,");
            sb.AppendLine("    [Double2] float NOT NULL,");
            sb.AppendLine("    [Decimal2] decimal(18,2) NOT NULL,");
            sb.AppendLine("    [Decimal3] decimal(20,4) NOT NULL,");
            sb.AppendLine("    [Guid2] uniqueidentifier NOT NULL,");
            sb.AppendLine("    [Short2] smallint NOT NULL,");
            sb.AppendLine("    [DateTime2] datetime2 NOT NULL,");
            sb.AppendLine("    [Bool2] bit NOT NULL,");
            sb.AppendLine("    [TimeSpan2] time NOT NULL,");
            sb.AppendLine("    [Byte2] tinyint NOT NULL,");
            sb.AppendLine("    [String2] nvarchar(100) NOT NULL,");
            sb.AppendLine("    [String3] nvarchar(max) NOT NULL,");
            sb.AppendLine("    CONSTRAINT PK_NotNullableTable2 PRIMARY KEY (Id)");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1].Body);

            sb.Clear();
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result[0].Descriptions.Count);
            Assert.Equal("EXEC sp_addextendedproperty 'MS_Description', N'test add column', 'schema', N'dbo', 'table', N'NullableTable', 'column', N'int3'", result[0].Descriptions[0]);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE dbo.[NullableTable] ADD [int3] int NULL", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE dbo.[SpecifiedMapTestTable] (");
            sb.AppendLine("    [NormalTxt] nvarchar(max) NULL,");
            sb.AppendLine("    [SpecifiedTxt] text NULL");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
        }



        [Fact, Priority(402)]
        public void TestSqlServer()
        {
            InitDatabase();
            TestRepository();

        }

        [Fact, Priority(401)]
        public async Task TestSqlServerAsync()
        {
            InitDatabase();
            await TestRepositoryAsync();
        }

        private void InitDatabase()
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

            InitService();
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
       
        private void InitService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
            var configurationRoot = build.Build();

            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSingleton<IConfiguration>(configurationRoot);
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("Sqlserver connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                it.AddDatabaseUnit<SqlConnection, IUnitOfWork1>(connectionString,
                    x =>
                    {
                        x.BindRepositoriesWithAttribute<SqlServerAutoRepositoryAttribute>();
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
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        }

        public async Task TestRepositoryAsync()
        {
            var uow = serviceProvider.GetService<IUnitOfWork1>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //Test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            await customerRepository.Where(it => it.Name == "testCustomer")
                .SetValue(it => it.Age, 5)
                .SetValue(it => it.TotalConsumptionAmount, 100)
                .ExecuteUpdateAsync();

            var ccc = customerRepository.Where(it => it.Name == "testCustomer").Distinct().FirstOrDefault();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single((IEnumerable)age5Customers);
            Assert.Equal(5, age5Customers[0].Age);
            Assert.Equal(100, age5Customers[0].TotalConsumptionAmount);

            var orderHeader = new OrderHeader();
            orderHeader.CreateTime = DateTime.UtcNow;
            orderHeader.CustomerId = customer.Id;
            orderHeader.State = 1;
            orderHeader.OrderNo = Guid.NewGuid().ToString("N");
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderDetail = new OrderDetail();
            orderDetail.OrderHeaderId = orderHeader.Id;
            orderDetail.ProductName = "apple";
            orderDetail.Quantity = 1;
            await orderDetailRepository.InsertAsync(orderDetail);

            var orderDetail2 = new OrderDetail();
            orderDetail2.OrderHeaderId = orderHeader.Id;
            orderDetail2.ProductName = "orange";
            orderDetail2.Quantity = 2;
            await orderDetailRepository.InsertAsync(orderDetail2);

            var result = await customerRepository.QueryAllBuyProductByNameAsync("testCustomer");
            Assert.Contains(result, t => t.ProductName == "apple");
            Assert.Contains(result, t => t.ProductName == "orange");

            orderDetail.Quantity = 2;
            await orderDetailRepository.UpdateAsync(orderDetail);
            var orderDetailTmp = await orderDetailRepository.GetAsync(orderDetail.Id);
            Assert.Equal(2, orderDetailTmp.Quantity);

            await orderDetailRepository.DeleteAsync(orderDetail2);
            var result2 = await customerRepository.QueryAllBuyProductByNameAsync("testCustomer");
            Assert.Single(result2);
            Assert.Contains(result2, t => t.ProductName == "apple");

            //Test unitOfWork
            try
            {
                uow.BeginTransaction();
                await customerRepository.InsertAsync(new Customer() { Name = "testCustomer2" });
                var orderDetail3 = new OrderDetail
                {
                    OrderHeaderId = orderHeader.Id,
                    ProductName = "ball",
                    Quantity = 3
                };
                await orderDetailRepository.InsertAsync(orderDetail3);
                uow.Commit();
            }
            catch (Exception e)
            {
                uow.RollBack();
            }

            var allCustomer = await customerRepository.GetAllAsync();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allCustomer, t => t.Name == "testCustomer2");
            var allOrderDetails = await orderDetailRepository.GetAllAsync();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allOrderDetails, t => t.ProductName == "ball");

            try
            {
                uow.BeginTransaction();
                await customerRepository.InsertAsync(new Customer() { Name = "testCustomer3" });
                throw new Exception("testException");
                var orderDetail4 = new OrderDetail();
                orderDetail4.OrderHeaderId = orderHeader.Id;
                orderDetail4.ProductName = "basketball";
                orderDetail4.Quantity = 4;
                await orderDetailRepository.InsertAsync(orderDetail4);
                uow.Commit();
            }
            catch (Exception e)
            {
                uow.RollBack();
            }
            allCustomer = await customerRepository.GetAllAsync();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allCustomer, t => t.Name == "testCustomer2");
            allOrderDetails = await orderDetailRepository.GetAllAsync();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allOrderDetails, t => t.ProductName == "ball");

            //Test page
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer() { Age = i, Name = "page" + i });
            }

            var newCount = await customerRepository.InsertAsync(customers);
            Assert.Equal(100, newCount.Count);
            var pageable = new Pageable(1, 10);
            var page = await customerRepository.GetCustomerByPageAsync(pageable, 5);
            //0-99岁，大于5的只有94个
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);
            var page2 = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();
            Assert.Equal(94, page2.TotalPages);
            Assert.Equal(10, page2.Data.Count);

            //lambda page
            var page3 = await customerRepository.Where(it => it.Age > 5).ToPageAsync(pageable);
            Assert.Equal(94, page3.TotalPages);
            Assert.Equal(10, page3.Data.Count);

            var maxAge = await customerRepository.MaxAsync(it => it.Age);
            Assert.Equal(99, maxAge);
            var minAge = await customerRepository.MinAsync(it => it.Age);
            Assert.Equal(0, minAge);
            var firstItem = await customerRepository.OrderBy(it => it.Age).FirstOrDefaultAsync();
            Assert.Equal(0, firstItem.Age);
            var firstItem2 = await customerRepository.OrderBy(it => it.Age).FirstAsync();
            Assert.Equal(0, firstItem2.Age);
            var firstItem3 = await customerRepository.OrderBy(it => it.Age).FirstOrDefaultAsync(it => it.Age > 5);
            Assert.Equal(6, firstItem3.Age);
            var firstItem4 = await customerRepository.OrderBy(it => it.Age).FirstAsync(it => it.Age > 5);
            Assert.Equal(6, firstItem4.Age);

            var totalCount = await customerRepository.CountAsync(it => it.Age > 5);
            Assert.Equal(94, totalCount);
            var sumResult = await customerRepository.Where(it => it.Age >= 98).SumAsync(it => it.Age);
            Assert.Equal(99 + 98, sumResult);
            var avgResult = await customerRepository.Where(it => it.Age >= 98).AverageAsync(it => it.Age);
            Assert.Equal((99 + 98) / 2, avgResult);

            //测试bindWhere构造条件
            var nameEmpty = WhereBuilder.Empty<string>();
            var ageEmpty = WhereBuilder.Empty<int>();
            var nameWhereItem = WhereBuilder.HasValue("page5");
            var ageWhereItem = WhereBuilder.HasValue(5);

            var bindResult = await customerRepository.GetCustomerByConditionAsync(nameWhereItem, ageEmpty);
            Assert.Single(bindResult);
            var bindResult2 = await customerRepository.GetCustomerByConditionAsync(nameEmpty, ageEmpty);
            Assert.Equal(102, bindResult2.Count);
            var bindResult3 = await customerRepository.GetCustomerByConditionAsync(nameEmpty, ageWhereItem);
            Assert.Equal(2, bindResult3.Count);
            var bindResult4 = await customerRepository.GetCustomerByConditionAsync(nameWhereItem, ageWhereItem);
            Assert.Single(bindResult4);
            var bindResult5 = await customerRepository.GetCustomerByPageByConditionAsync(pageable, nameWhereItem, ageEmpty);
            Assert.Single(bindResult5.Data);
            var bindResult6 = await customerRepository.GetCustomerByPageByConditionAsync(pageable, nameEmpty, ageEmpty);
            Assert.Equal(10, bindResult6.Data.Count);
            var bindResult7 = await customerRepository.GetCustomerByPageByConditionAsync(pageable, nameEmpty, ageWhereItem);
            Assert.Equal(2, bindResult7.Data.Count);
            var bindResult8 = await customerRepository.GetCustomerByPageByConditionAsync(pageable, nameWhereItem, ageWhereItem);
            Assert.Single(bindResult8.Data);

            //var customers= customerRepository.Where(it => it.Age > 5).OrderBy(it => it.Id).Take(10).ToList();

            //Test update 
            var newCount2 = await customerRepository.Where(it => it.Age > 5).SetValue(it => it.Name, "a")
                .ExecuteUpdateAsync();
            Assert.Equal(94, newCount2);
            //Test delete 
            var newCount3 = await customerRepository.DeleteAsync(it => it.Age > 5);
            Assert.Equal(94, newCount3);
            await customerRepository.DeleteAsync(it => it.Age > 5);
            var newCount4 = await customerRepository.GetAllAsync();
            Assert.Equal(8, newCount4.Count);
            await customerRepository.InsertAsync(new Customer() { Age = 200, Name = null });
            var emptyNameCustomers = await customerRepository.Where(it => it.Name == null).ToListAsync();
            Assert.Equal(1, emptyNameCustomers.Count);
            var notNullNameCustomers = await customerRepository.Where(it => it.Name != null).ToListAsync();
            Assert.Equal(8, notNullNameCustomers.Count);
            //测试空列表
            var parameters = new List<string>();
            var emptyCustomers = await customerRepository.Where(it => parameters.Contains(it.Name)).ToListAsync();
            Assert.Empty(emptyCustomers);
        }

        private void test(Expression<Func<Customer, object>> exp, object value)
        {

        }

        public void TestRepository()
        {
            var uow = serviceProvider.GetService<IUnitOfWork1>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            //var customCustomerRepository = serviceProvider.GetService<ICustomCustomerRepository>();

            //Test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            customerRepository.Insert(customer);

            var updateCount = customerRepository.Where(it => it.Name == "testCustomer")
                 .SetValue(it => it.Age, 5)
                 .SetValue(it => it.TotalConsumptionAmount, 100)
                 .ExecuteUpdate();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single((IEnumerable)age5Customers);
            Assert.Equal(5, age5Customers[0].Age);
            Assert.Equal(100, age5Customers[0].TotalConsumptionAmount);

            var orderHeader = new OrderHeader
            {
                CreateTime = DateTime.UtcNow,
                CustomerId = customer.Id,
                State = 1,
                OrderNo = Guid.NewGuid().ToString("N")
            };
            orderHeaderRepository.Insert(orderHeader);

            var orderDetail = new OrderDetail
            {
                OrderHeaderId = orderHeader.Id,
                ProductName = "apple",
                Quantity = 1
            };
            orderDetailRepository.Insert(orderDetail);

            var orderDetail2 = new OrderDetail
            {
                OrderHeaderId = orderHeader.Id,
                ProductName = "orange",
                Quantity = 2
            };
            orderDetailRepository.Insert(orderDetail2);

            var result = customerRepository.QueryAllBuyProductByName("testCustomer");
            Assert.Contains(result, t => t.ProductName == "apple");
            Assert.Contains(result, t => t.ProductName == "orange");

            orderDetail.Quantity = 2;
            orderDetailRepository.Update(orderDetail);
            var orderDetailTmp = orderDetailRepository.Get(orderDetail.Id);
            Assert.Equal(2, orderDetailTmp.Quantity);

            orderDetailRepository.Delete(orderDetail2);
            var result2 = customerRepository.QueryAllBuyProductByName("testCustomer");
            Assert.Single(result2);
            Assert.Contains(result2, t => t.ProductName == "apple");

            //Test unitOfWork
            try
            {
                uow.BeginTransaction();
                customerRepository.Insert(new Customer() { Name = "testCustomer2" });
                var orderDetail3 = new OrderDetail();
                orderDetail3.OrderHeaderId = orderHeader.Id;
                orderDetail3.ProductName = "ball";
                orderDetail3.Quantity = 3;
                orderDetailRepository.Insert(orderDetail3);
                uow.Commit();
            }
            catch (Exception e)
            {
                uow.RollBack();
            }

            var allCustomer = customerRepository.GetAll();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allCustomer, t => t.Name == "testCustomer2");
            var allOrderDetails = orderDetailRepository.GetAll();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allOrderDetails, t => t.ProductName == "ball");

            try
            {
                uow.BeginTransaction();
                customerRepository.Insert(new Customer() { Name = "testCustomer3" });
                throw new Exception("testException");
            }
            catch (Exception e)
            {
                uow.RollBack();
            }
            allCustomer = customerRepository.GetAll();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allCustomer, t => t.Name == "testCustomer2");
            allOrderDetails = orderDetailRepository.GetAll();
            Assert.Equal(2, allCustomer.Count);
            Assert.Contains(allOrderDetails, t => t.ProductName == "ball");

            // Test page
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer() { Age = i, Name = "page" + i });
            }

            var newCount = customerRepository.Insert(customers);
            //Assert.Equal(100, newCount);
            var pageable = new Pageable(1, 10);
            var page = customerRepository.GetCustomerByPage(pageable, 5);
            //0-99岁，大于5的只有94个
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);
            var page2 = customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPage();
            Assert.Equal(94, page2.TotalPages);
            Assert.Equal(10, page2.Data.Count);

            //lambda page
            var page3 = customerRepository.Where(it => it.Age > 5).ToPage(pageable);
            Assert.Equal(94, page3.TotalPages);
            Assert.Equal(10, page3.Data.Count);

            var maxAge = customerRepository.Max(it => it.Age);
            Assert.Equal(99, maxAge);
            var minAge = customerRepository.Min(it => it.Age);
            Assert.Equal(0, minAge);
            var firstItem = customerRepository.OrderBy(it => it.Age).FirstOrDefault();
            Assert.Equal(0, firstItem.Age);
            var firstItem2 = customerRepository.OrderBy(it => it.Age).First();
            Assert.Equal(0, firstItem2.Age);
            var firstItem3 = customerRepository.OrderBy(it => it.Age).FirstOrDefault(it => it.Age > 5);
            Assert.Equal(6, firstItem3.Age);
            var firstItem4 = customerRepository.OrderBy(it => it.Age).First(it => it.Age > 5);
            Assert.Equal(6, firstItem4.Age);

            var totalCount = customerRepository.Count(it => it.Age > 5);
            Assert.Equal(94, totalCount);
            var sumResult = customerRepository.Where(it => it.Age >= 98).Sum(it => it.Age);
            Assert.Equal(99 + 98, sumResult);
            var avgResult = customerRepository.Where(it => it.Age >= 98).Average(it => it.Age);
            Assert.Equal((99 + 98) / 2, avgResult);

            //测试bindWhere构造条件
            var nameEmpty = WhereBuilder.Empty<string>();//
            var ageEmpty = WhereBuilder.Empty<int>();
            var nameWhereItem = WhereBuilder.HasValue("page5");
            var ageWhereItem = WhereBuilder.HasValue(5);

            var bindResult = customerRepository.GetCustomerByCondition(nameWhereItem, ageEmpty);
            Assert.Single(bindResult);
            var bindResult2 = customerRepository.GetCustomerByCondition(nameEmpty, ageEmpty);
            Assert.Equal(102, bindResult2.Count);
            var bindResult3 = customerRepository.GetCustomerByCondition(nameEmpty, ageWhereItem);
            Assert.Equal(2, bindResult3.Count);
            var bindResult4 = customerRepository.GetCustomerByCondition(nameWhereItem, ageWhereItem);
            Assert.Single(bindResult4);
            var bindResult5 = customerRepository.GetCustomerByPageByCondition(pageable, nameWhereItem, ageEmpty);
            Assert.Single(bindResult5.Data);
            var bindResult6 = customerRepository.GetCustomerByPageByCondition(pageable, nameEmpty, ageEmpty);
            Assert.Equal(10, bindResult6.Data.Count);
            var bindResult7 = customerRepository.GetCustomerByPageByCondition(pageable, nameEmpty, ageWhereItem);
            Assert.Equal(2, bindResult7.Data.Count);
            var bindResult8 = customerRepository.GetCustomerByPageByCondition(pageable, nameWhereItem, ageWhereItem);
            Assert.Single(bindResult8.Data);


            //测试firstOrDefault
            var firstOrDefaultResult = customerRepository.FirstOrDefault(it => it.Name == "page5");
            Assert.NotNull(firstOrDefaultResult);
            var firstOrDefaultResult2 = customerRepository.First(it => it.Name == "page5");
            Assert.NotNull(firstOrDefaultResult2);

            //Test update 
            var newCount2 = customerRepository.Where(it => it.Age > 5).SetValue(it => it.Name, "a")
                .ExecuteUpdate();
            Assert.Equal(94, newCount2);
            //Test delete 
            var newCount3 = customerRepository.Delete(it => it.Age > 5);
            Assert.Equal(94, newCount3);
            customerRepository.Delete(it => it.Age > 5);
            var newCount4 = customerRepository.GetAll();
            Assert.Equal(8, newCount4.Count);
            customerRepository.Insert(new Customer() { Age = 200, Name = null });
            var emptyNameCustomers = customerRepository.Where(it => it.Name == null).ToList();
            Assert.Equal(1, emptyNameCustomers.Count);
            var notNullNameCustomers = customerRepository.Where(it => it.Name != null).ToList();
            Assert.Equal(8, notNullNameCustomers.Count);

            //测试空列表
            var parameters = new List<string>();
            var emptyCustomers = customerRepository.Where(it => parameters.Contains(it.Name)).ToList();
            Assert.Empty(emptyCustomers);
        }

        public void TestLinq()
        {
            var uow = serviceProvider.GetService<IUnitOfWork1>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //Test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            customerRepository.Insert(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            customerRepository.Insert(customer2);

            var d = customerRepository.FirstOrDefault();
            var d1 = customerRepository.Where(it => it.Name.Contains("testCustomer")).ToList();

        }

        public void TestBaseQuery()
        {
            var uow = serviceProvider.GetService<IUnitOfWork1>();
            var orderQueryRepository = serviceProvider.GetService<IOrderQueryRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //Test insert,update,get,delete 
            var orderHeader = new OrderHeader();
            orderHeader.CreateTime = DateTime.UtcNow;
            orderHeader.State = 1;
            orderHeader.OrderNo = Guid.NewGuid().ToString("N");
            orderHeaderRepository.Insert(orderHeader);


            var orderDetail = new OrderDetail
            {
                OrderHeaderId = orderHeader.Id,
                ProductName = "apple",
                Quantity = 1
            };
            orderDetailRepository.Insert(orderDetail);

            var orderDetail2 = new OrderDetail
            {
                OrderHeaderId = orderHeader.Id,
                ProductName = "orange",
                Quantity = 2
            };
            orderDetailRepository.Insert(orderDetail2);

            var r1 = orderQueryRepository.GetOrderQuery();
            var r2 = orderQueryRepository.GetOrderQueryList();
        }
    }
}
