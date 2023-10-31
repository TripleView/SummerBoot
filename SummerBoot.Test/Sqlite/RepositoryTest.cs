using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Sqlite.Db;
using SummerBoot.Test.Sqlite.Models;
using SummerBoot.Test.Sqlite.Repository;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SummerBoot.Test.Common.Dto;
using SummerBoot.Test.Sqlite.Dto;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.Sqlite
{
    public interface ITest
    {

    }

    [AutoRegister(typeof(ITest),ServiceLifetime.Transient)]
    public class Test:ITest
    {

    }

    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 测试where条件的各种情况
        /// </summary>
        [Fact, Priority(322)]
        public async Task TestWhereCombine()
        {
            InitSqliteDatabase("Data Source=./TestWhereCombinee.db");

            var addressRepository = serviceProvider.GetService<IAddressRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

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
                Name = "bob",
                CustomerNo = "A2",
                TotalConsumptionAmount = 2
            };
            await customerRepository.InsertAsync(customer1);
            await customerRepository.InsertAsync(customer2);

            #region 测试单表where

            var customerList1 = await customerRepository.Where(it => it.Name == "bob").ToListAsync();
            Assert.Equal(2, customerList1.Count);
            var customerList2 = await customerRepository.Where(it => it.Name == "bob").Where(it => it.Age == 1).ToListAsync();
            Assert.Equal(1, customerList2.Count);

            var customerList3 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhere(it => it.Age == 1).ToListAsync();
            Assert.Equal(2, customerList3.Count);

            var customerList4 = await customerRepository.WhereIf(true, it => it.CustomerNo == "A2").ToListAsync();
            Assert.Equal(1, customerList4.Count);

            var customerList5 = await customerRepository.WhereIf(false, it => it.CustomerNo == "A2").ToListAsync();
            Assert.Equal(2, customerList5.Count);

            var customerList6 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhereIf(true, it => it.Age == 1).ToListAsync();
            Assert.Equal(2, customerList6.Count);

            var customerList7 = await customerRepository.Where(it => it.CustomerNo == "A2").OrWhereIf(false, it => it.Age == 1).ToListAsync();
            Assert.Equal(1, customerList7.Count);

            var customerList8 = await customerRepository.OrWhere(it => it.CustomerNo == "A2").ToListAsync();
            Assert.Equal(1, customerList8.Count);
            #endregion


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
                CustomerId = customer1.Id,
                City = "B",
                CreateOn = date4
            };
            var address3 = new Address()
            {
                CustomerId = customer2.Id,
                City = "A",
                CreateOn = date4
            };
            await addressRepository.InsertAsync(address1);
            await addressRepository.InsertAsync(address2);
            await addressRepository.InsertAsync(address3);

            var firstBuyDate = new DateTime(2023, 8, 1, 17, 0, 0);
            var secondBuyDate = new DateTime(2023, 10, 26, 17, 0, 0);

            var orderHeader = new OrderHeader()
            {
                CreateTime = firstBuyDate,
                CustomerId = customer1.Id,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader1 = new OrderHeader()
            {
                CreateTime = secondBuyDate,
                CustomerId = customer1.Id,
                OrderNo = "JJJ",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader1);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = firstBuyDate,
                CustomerId = customer2.Id,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderDetail3 = new OrderDetail()
            {
                OrderHeaderId = orderHeader2.Id,
                ProductName = "C",
                Quantity = 3,
                State = 1
            };

            var orderDetail4 = new OrderDetail()
            {
                OrderHeaderId = orderHeader1.Id,
                ProductName = "D",
                Quantity = 4,
                State = 1
            };
            await orderDetailRepository.InsertAsync(orderDetail1);
            await orderDetailRepository.InsertAsync(orderDetail2);
            await orderDetailRepository.InsertAsync(orderDetail3);
            await orderDetailRepository.InsertAsync(orderDetail4);
            #region 测试双表where

            var result = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T2.City == "A")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result.Count);

            var result11 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .OrWhere(it => it.T2.City == "A")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result11.Count);

            var result111 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId && it.T2.City.StartsWith("A"))
                .OrWhere(it => it.T2.City == "A")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result111.Count);

            var result2 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T2.City == "A")
                .Where(it => it.T1.CustomerNo == "A1")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(1, result2.Count);

            var result3 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .WhereIf(true, it => it.T2.City == "A")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result3.Count);

            var result4 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .WhereIf(false, it => it.T2.City == "A")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(3, result4.Count);

            var result5 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T1.CustomerNo == "A2")
                .OrWhere(it => it.T2.City == "B")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result5.Count);

            var result6 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T1.CustomerNo == "A2")
                .OrWhereIf(true, it => it.T2.City == "B")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(2, result6.Count);

            var result7 = await customerRepository
                .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T1.CustomerNo == "A2")
                .OrWhereIf(false, it => it.T2.City == "B")
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(1, result7.Count);
            #endregion

            #region 测试3表where
            //测试多join和where条件
            //  .InnerJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
            var orderList = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList.Count);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("D", orderList[2].ProductName);

            var orderList1 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T3.CustomerNo == "A1")
                .Where(it => it.T1.CreateTime == firstBuyDate)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(2, orderList1.Count);
            Assert.Equal("A", orderList1[0].ProductName);
            Assert.Equal("B", orderList1[1].ProductName);


            var orderList3 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .OrWhere(it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList3.Count);
            Assert.Equal("A", orderList3[0].ProductName);
            Assert.Equal("B", orderList3[1].ProductName);
            Assert.Equal("D", orderList3[2].ProductName);

            var orderList4 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .WhereIf(true, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList4.Count);
            Assert.Equal("A", orderList4[0].ProductName);
            Assert.Equal("B", orderList4[1].ProductName);
            Assert.Equal("D", orderList4[2].ProductName);

            var orderList5 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .WhereIf(false, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(4, orderList5.Count);

            var orderList6 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .OrWhereIf(true, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList6.Count);
            Assert.Equal("A", orderList3[0].ProductName);
            Assert.Equal("B", orderList3[1].ProductName);
            Assert.Equal("D", orderList3[2].ProductName);

            var orderList7 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .OrWhereIf(false, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(4, orderList7.Count);
            Assert.Equal("A", orderList7[0].ProductName);
            Assert.Equal("B", orderList7[1].ProductName);
            Assert.Equal("C", orderList7[2].ProductName);
            Assert.Equal("D", orderList7[3].ProductName);
            #endregion

            #region 测试4表where
            //测试多join和where条件
            //  .InnerJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
            var orderList8 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList8.Count);
            Assert.Equal("A", orderList8[0].ProductName);
            Assert.Equal("A", orderList8[0].CustomerCity);
            Assert.Equal("B", orderList8[1].ProductName);
            Assert.Equal("D", orderList8[2].ProductName);

            var orderList9 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T3.CustomerNo == "A1")
                .Where(it => it.T1.CreateTime == firstBuyDate)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(2, orderList9.Count);
            Assert.Equal("A", orderList9[0].ProductName);
            Assert.Equal("A", orderList9[0].CustomerCity);
            Assert.Equal("B", orderList9[1].ProductName);


            var orderList10 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .OrWhere(it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList10.Count);
            Assert.Equal("A", orderList10[0].ProductName);
            Assert.Equal("A", orderList10[0].CustomerCity);
            Assert.Equal("B", orderList10[1].ProductName);
            Assert.Equal("D", orderList10[2].ProductName);

            var orderList11 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .WhereIf(true, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList11.Count);
            Assert.Equal("A", orderList11[0].ProductName);
            Assert.Equal("A", orderList11[0].CustomerCity);
            Assert.Equal("B", orderList11[1].ProductName);
            Assert.Equal("D", orderList11[2].ProductName);

            var orderList12 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .WhereIf(false, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList12.Count);

            var orderList13 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .OrWhereIf(true, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList13.Count);
            Assert.Equal("A", orderList13[0].ProductName);
            Assert.Equal("A", orderList13[0].CustomerCity);
            Assert.Equal("B", orderList13[1].ProductName);
            Assert.Equal("D", orderList13[2].ProductName);

            var orderList14 = await orderHeaderRepository
                .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .InnerJoin(new Address(), it => it.T4.CustomerId == it.T3.Id && it.T4.CreateOn == date3)
                .OrderBy(it => it.T2.Quantity)
                .OrWhereIf(false, it => it.T3.CustomerNo == "A1")
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(3, orderList14.Count);
            Assert.Equal("A", orderList14[0].ProductName);
            Assert.Equal("A", orderList14[0].CustomerCity);
            Assert.Equal("B", orderList14[1].ProductName);
            Assert.Equal("D", orderList14[2].ProductName);

            #endregion
        }

        /// <summary>
        /// 测试where条件中右边的值为dto的属性，并且同时测试从list里获取索引为0或者1的值
        /// </summary>
        [Fact, Priority(321)]
        public async Task TestParameterWithListPropertyDtoAndGetItem()
        {
            InitSqliteDatabase("Data Source=./TestParameterWithListPropertyDtoAndGetItem.db");

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

        /// <summary>
        /// 测试3张表联查
        /// </summary>
        [Fact, Priority(320)]
        public async Task TestLeftJoin4TableAsync()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin4TableAsync.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var addressRepository = serviceProvider.GetService<IAddressRepository>();
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
            var address1 = new Address()
            {
                CustomerId = customer1.Id,
                City = "A"
            };
            var address2 = new Address()
            {
                CustomerId = customer2.Id,
                City = "B"
            };
            await addressRepository.InsertAsync(address1);
            await addressRepository.InsertAsync(address2);

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer1.Id,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer2.Id,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToListAsync();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal("A", orderList[0].CustomerCity);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal("A", orderList[1].CustomerCity);
            Assert.Equal(1, orderList[1].Age);

            //test anonymous type
            var orderList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
                .ToListAsync();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal("A", orderList2[0].CustomerCity);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);
            Assert.Equal("A", orderList2[1].CustomerCity);


            var orderAddresses = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T4)
                .ToListAsync();
            Assert.Equal(2, orderAddresses.Count);
            Assert.Equal("A", orderAddresses[1].City);

            ////测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToPageAsync(pageable);
            orderList = orderPageList.Data;
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal("A", orderList[0].CustomerCity);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);
            Assert.Equal("A", orderList[1].CustomerCity);

            ////test anonymous type
            var orderPageList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
                .ToPageAsync(pageable);
            var orderList3 = orderPageList2.Data;
            Assert.Equal(2, orderList3.Count);
            Assert.Equal(1, orderList3[0].Quantity);
            Assert.Equal("A", orderList3[0].ProductName);
            Assert.Equal("ABC", orderList3[0].OrderNo);
            Assert.Equal("A1", orderList3[0].CustomerNo);
            Assert.Equal(1, orderList3[0].Age);
            Assert.Equal("A", orderList3[0].CustomerCity);
            Assert.Equal(2, orderList3[1].Quantity);
            Assert.Equal("B", orderList3[1].ProductName);
            Assert.Equal("ABC", orderList3[1].OrderNo);
            Assert.Equal("A1", orderList3[1].CustomerNo);
            Assert.Equal(1, orderList3[1].Age);
            Assert.Equal("A", orderList3[1].CustomerCity);

            var orderAddressPages = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T4)
                .ToPageAsync(pageable);

            orderAddresses = orderAddressPages.Data;
            Assert.Equal(2, orderAddresses.Count);
            Assert.Equal("A", orderAddresses[1].City);
        }

        /// <summary>
        /// 测试3张表联查
        /// </summary>
        [Fact, Priority(319)]
        public async Task TestLeftJoin3TableAsync()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin3TableAsync.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
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

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer1.Id,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer2.Id,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToListAsync();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);

            //test anonymous type
            var orderList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
                .ToListAsync();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);


            var orderCustomers = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T3)
                .ToListAsync();
            Assert.Equal(2, orderCustomers.Count);
            Assert.Equal("A1", orderCustomers[1].CustomerNo);
            Assert.Equal("bob", orderCustomers[1].Name);

            ////测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToPageAsync(pageable);
            orderList = orderPageList.Data;
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);

            ////test anonymous type
            var orderPageList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
                .ToPageAsync(pageable);
            orderList2 = orderPageList2.Data;
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);

            var orderCustomerPages = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T3)
                .ToPageAsync(pageable);
            orderCustomers = orderCustomerPages.Data;
            Assert.Equal(2, orderCustomers.Count);
            Assert.Equal("A1", orderCustomers[1].CustomerNo);
            Assert.Equal("bob", orderCustomers[1].Name);
        }
        /// <summary>
        /// 测试2张表联查
        /// </summary>
        [Fact, Priority(318)]
        public async Task TestLeftJoin2TableAsync()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin2TableAsync.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = 1,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = 2,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
                .ToListAsync();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal(1, orderList[1].CustomerId);

            //test anonymous type
            var orderList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
                .ToListAsync();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal(1, orderList2[1].CustomerId);


            var orderDetails = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderByDescending(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T2)
                .ToListAsync();
            Assert.Equal(2, orderDetails.Count);
            Assert.Equal(1, orderDetails[1].Quantity);
            Assert.Equal("A", orderDetails[1].ProductName);
            Assert.Equal(2, orderDetails[0].Quantity);
            Assert.Equal("B", orderDetails[0].ProductName);

            //测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
                .ToPageAsync(pageable);

            Assert.Equal(2, orderPageList.TotalPages);
            orderList = orderPageList.Data;
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal(1, orderList[1].CustomerId);

            //test anonymous type
            var orderPageList2 = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
                .ToPageAsync(pageable);

            Assert.Equal(2, orderPageList2.TotalPages);
            orderList2 = orderPageList2.Data;
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal(1, orderList2[1].CustomerId);

            var orderPageDetails = await orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderByDescending(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T2)
                .ToPageAsync(pageable);
            Assert.Equal(2, orderPageDetails.TotalPages);
            orderDetails = orderPageDetails.Data;
            Assert.Equal(2, orderDetails.Count);
            Assert.Equal(1, orderDetails[1].Quantity);
            Assert.Equal("A", orderDetails[1].ProductName);
            Assert.Equal(2, orderDetails[0].Quantity);
            Assert.Equal("B", orderDetails[0].ProductName);
        }

        /// <summary>
        /// 测试3张表联查
        /// </summary>
        [Fact, Priority(417)]
        public async Task TestLeftJoin4Table()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin4Table.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var addressRepository = serviceProvider.GetService<IAddressRepository>();
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
            var address1 = new Address()
            {
                CustomerId = customer1.Id,
                City = "A"
            };
            var address2 = new Address()
            {
                CustomerId = customer2.Id,
                City = "B"
            };
            await addressRepository.InsertAsync(address1);
            await addressRepository.InsertAsync(address2);

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer1.Id,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer2.Id,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToList();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal("A", orderList[0].CustomerCity);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal("A", orderList[1].CustomerCity);
            Assert.Equal(1, orderList[1].Age);

            //test anonymous type
            var orderList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
                .ToList();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal("A", orderList2[0].CustomerCity);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);
            Assert.Equal("A", orderList2[1].CustomerCity);


            var orderAddresses = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T4)
                .ToList();
            Assert.Equal(2, orderAddresses.Count);
            Assert.Equal("A", orderAddresses[1].City);

            ////测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
                .ToPage(pageable);
            orderList = orderPageList.Data;
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal("A", orderList[0].CustomerCity);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);
            Assert.Equal("A", orderList[1].CustomerCity);

            ////test anonymous type
            var orderPageList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City })
                .ToPage(pageable);
            var orderList3 = orderPageList2.Data;
            Assert.Equal(2, orderList3.Count);
            Assert.Equal(1, orderList3[0].Quantity);
            Assert.Equal("A", orderList3[0].ProductName);
            Assert.Equal("ABC", orderList3[0].OrderNo);
            Assert.Equal("A1", orderList3[0].CustomerNo);
            Assert.Equal(1, orderList3[0].Age);
            Assert.Equal("A", orderList3[0].CustomerCity);
            Assert.Equal(2, orderList3[1].Quantity);
            Assert.Equal("B", orderList3[1].ProductName);
            Assert.Equal("ABC", orderList3[1].OrderNo);
            Assert.Equal("A1", orderList3[1].CustomerNo);
            Assert.Equal(1, orderList3[1].Age);
            Assert.Equal("A", orderList3[1].CustomerCity);

            var orderAddressPages = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T4)
                .ToPage(pageable);

            orderAddresses = orderAddressPages.Data;
            Assert.Equal(2, orderAddresses.Count);
            Assert.Equal("A", orderAddresses[1].City);
        }

        /// <summary>
        /// 测试3张表联查
        /// </summary>
        [Fact, Priority(316)]
        public async Task TestLeftJoin3Table()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin3Table.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
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

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer1.Id,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = customer2.Id,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToList();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);

            //test anonymous type
            var orderList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
                .ToList();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);


            var orderCustomers = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T3)
                .ToList();
            Assert.Equal(2, orderCustomers.Count);
            Assert.Equal("A1", orderCustomers[1].CustomerNo);
            Assert.Equal("bob", orderCustomers[1].Name);

            ////测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age }, x => x.T1)
                .ToPage(pageable);
            orderList = orderPageList.Data;
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal("A1", orderList[0].CustomerNo);
            Assert.Equal(1, orderList[0].Age);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal("A1", orderList[1].CustomerNo);
            Assert.Equal(1, orderList[1].Age);

            ////test anonymous type
            var orderPageList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age })
                .ToPage(pageable);
            orderList2 = orderPageList2.Data;
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal("A1", orderList2[0].CustomerNo);
            Assert.Equal(1, orderList2[0].Age);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal("A1", orderList2[1].CustomerNo);
            Assert.Equal(1, orderList2[1].Age);

            var orderCustomerPages = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
                .OrderByDescending(it => it.T3.Age)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T3)
                .ToPage(pageable);
            orderCustomers = orderCustomerPages.Data;
            Assert.Equal(2, orderCustomers.Count);
            Assert.Equal("A1", orderCustomers[1].CustomerNo);
            Assert.Equal("bob", orderCustomers[1].Name);
        }
        /// <summary>
        /// 测试2张表联查
        /// </summary>
        [Fact, Priority(315)]
        public async Task TestLeftJoin2Table()
        {
            InitSqliteDatabase("Data Source=./TestLeftJoin2Table.db");
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = 1,
                OrderNo = "ABC",
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                CustomerId = 2,
                OrderNo = "DEF",
                State = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
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

            var orderList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
                .ToList();
            Assert.Equal(2, orderList.Count);
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal(1, orderList[1].CustomerId);

            //test anonymous type
            var orderList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
                .ToList();
            Assert.Equal(2, orderList2.Count);
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal(1, orderList2[1].CustomerId);


            var orderDetails = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderByDescending(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T2)
                .ToList();
            Assert.Equal(2, orderDetails.Count);
            Assert.Equal(1, orderDetails[1].Quantity);
            Assert.Equal("A", orderDetails[1].ProductName);
            Assert.Equal(2, orderDetails[0].Quantity);
            Assert.Equal("B", orderDetails[0].ProductName);

            //测试分页
            var pageable = new Pageable(1, 5);
            var orderPageList = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity }, x => x.T1)
                .ToPage(pageable);

            Assert.Equal(2, orderPageList.TotalPages);
            orderList = orderPageList.Data;
            Assert.Equal(1, orderList[0].Quantity);
            Assert.Equal("A", orderList[0].ProductName);
            Assert.Equal("ABC", orderList[0].OrderNo);
            Assert.Equal(2, orderList[1].Quantity);
            Assert.Equal("B", orderList[1].ProductName);
            Assert.Equal("ABC", orderList[1].OrderNo);
            Assert.Equal(1, orderList[1].CustomerId);

            //test anonymous type
            var orderPageList2 = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderBy(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => new { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, OrderNo = it.T1.OrderNo, CustomerId = it.T1.CustomerId })
                .ToPage(pageable);

            Assert.Equal(2, orderPageList2.TotalPages);
            orderList2 = orderPageList2.Data;
            Assert.Equal(1, orderList2[0].Quantity);
            Assert.Equal("A", orderList2[0].ProductName);
            Assert.Equal("ABC", orderList2[0].OrderNo);
            Assert.Equal(2, orderList2[1].Quantity);
            Assert.Equal("B", orderList2[1].ProductName);
            Assert.Equal("ABC", orderList2[1].OrderNo);
            Assert.Equal(1, orderList2[1].CustomerId);

            var orderPageDetails = orderHeaderRepository
                .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
                .OrderByDescending(it => it.T2.Quantity)
                .Where(it => it.T1.State == 1)
                .Select(it => it.T2)
                .ToPage(pageable);
            Assert.Equal(2, orderPageDetails.TotalPages);
            orderDetails = orderPageDetails.Data;
            Assert.Equal(2, orderDetails.Count);
            Assert.Equal(1, orderDetails[1].Quantity);
            Assert.Equal("A", orderDetails[1].ProductName);
            Assert.Equal(2, orderDetails[0].Quantity);
            Assert.Equal("B", orderDetails[0].ProductName);
        }

        /// <summary>
        /// 测试插入实体和更新实体前的自定义函数
        /// </summary>
        [Fact, Priority(310)]
        public async Task TestBeforeInsertAndUpdateEvent()
        {
            InitSqliteDatabase("Data Source=./TestBeforeInsertAndUpdateEvent.db");
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
        [Fact, Priority(309)]
        public async Task TestModelUseGuidAsId()
        {
            InitSqliteDatabase("Data Source=./TestModelUseGuidAsId.db");
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
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(308)]
        public async Task TestGetSqlByConfigurationAsync()
        {
            InitSqliteDatabase("Data Source=./TestGetSqlByConfigurationAsync.db");
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
        [Fact, Priority(307)]
        public void TestGetSqlByConfiguration()
        {
            InitSqliteDatabase("Data Source=./TestGetSqlByConfiguration.db");
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
        /// 测试根据实体类创建数据库表和进行插入查询对照
        /// </summary>
        [Fact, Priority(306)]
        public void TestCreateTableFromEntityAndCrud()
        {
            InitSqliteDatabase("Data Source=./TestCreateTableFromEntityAndCrud.db");
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
                TimeSpan2 = TimeSpan.FromDays(1)
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
        [Fact, Priority(305)]
        public void TestTableColumnMap()
        {
            InitSqliteDatabase("Data Source=./TestTableColumnMap.db");
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var tableColumnMapRepository = serviceProvider.GetService<ITableColumnMapRepository>();
            customerRepository.Insert(new Customer() { Name = "sb" });
            var customer = tableColumnMapRepository.FirstOrDefault(it => it.CustomerName == "sb");
            Assert.NotNull(customer);
            Assert.Equal("sb", customer.CustomerName);
        }

        [Fact, Priority(304)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitSqliteDatabase("Data Source=./TestGenerateCsharpClassByDatabaseInfo.db");
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
            sb.AppendLine("      public long Id { get; set; }");
            sb.AppendLine("      [Column(\"Name\")]");
            sb.AppendLine("      public string Name { get; set; }");
            sb.AppendLine("      [Column(\"Age\")]");
            sb.AppendLine("      public long Age { get; set; }");
            sb.AppendLine("      [Column(\"CustomerNo\")]");
            sb.AppendLine("      public string CustomerNo { get; set; }");
            sb.AppendLine("      [Column(\"TotalConsumptionAmount\")]");
            sb.AppendLine("      public string TotalConsumptionAmount { get; set; }");
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
            sb.AppendLine("   [Table(\"NullableTable\")]");
            sb.AppendLine("   public class NullableTable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"Id\")]");
            sb.AppendLine("      public long Id { get; set; }");
            sb.AppendLine("      [Column(\"Int2\")]");
            sb.AppendLine("      public long? Int2 { get; set; }");
            sb.AppendLine("      [Column(\"Long2\")]");
            sb.AppendLine("      public long? Long2 { get; set; }");
            sb.AppendLine("      [Column(\"Float2\")]");
            sb.AppendLine("      public double? Float2 { get; set; }");
            sb.AppendLine("      [Column(\"Double2\")]");
            sb.AppendLine("      public double? Double2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal2\")]");
            sb.AppendLine("      public string Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal3\")]");
            sb.AppendLine("      public string Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"Guid2\")]");
            sb.AppendLine("      public string Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"Short2\")]");
            sb.AppendLine("      public long? Short2 { get; set; }");
            sb.AppendLine("      [Column(\"DateTime2\")]");
            sb.AppendLine("      public string DateTime2 { get; set; }");
            sb.AppendLine("      [Column(\"Bool2\")]");
            sb.AppendLine("      public long? Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"TimeSpan2\")]");
            sb.AppendLine("      public string TimeSpan2 { get; set; }");
            sb.AppendLine("      [Column(\"Byte2\")]");
            sb.AppendLine("      public long? Byte2 { get; set; }");
            sb.AppendLine("      [Column(\"String2\")]");
            sb.AppendLine("      public string String2 { get; set; }");
            sb.AppendLine("      [Column(\"String3\")]");
            sb.AppendLine("      public string String3 { get; set; }");
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
            sb.AppendLine("   [Table(\"NotNullableTable\")]");
            sb.AppendLine("   public class NotNullableTable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"Id\")]");
            sb.AppendLine("      public long Id { get; set; }");
            sb.AppendLine("      [Column(\"Int2\")]");
            sb.AppendLine("      public long Int2 { get; set; }");
            sb.AppendLine("      [Column(\"Long2\")]");
            sb.AppendLine("      public long Long2 { get; set; }");
            sb.AppendLine("      [Column(\"Float2\")]");
            sb.AppendLine("      public double Float2 { get; set; }");
            sb.AppendLine("      [Column(\"Double2\")]");
            sb.AppendLine("      public double Double2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal2\")]");
            sb.AppendLine("      public string Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"Decimal3\")]");
            sb.AppendLine("      public string Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"Guid2\")]");
            sb.AppendLine("      public string Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"Short2\")]");
            sb.AppendLine("      public long Short2 { get; set; }");
            sb.AppendLine("      [Column(\"DateTime2\")]");
            sb.AppendLine("      public string DateTime2 { get; set; }");
            sb.AppendLine("      [Column(\"Bool2\")]");
            sb.AppendLine("      public long Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"TimeSpan2\")]");
            sb.AppendLine("      public string TimeSpan2 { get; set; }");
            sb.AppendLine("      [Column(\"Byte2\")]");
            sb.AppendLine("      public long Byte2 { get; set; }");
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
        [Fact, Priority(303)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {

            InitSqliteDatabase("Data Source=./TestGenerateDatabaseTableByCsharpClass.db");
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE `NullableTable2` (");
            sb.AppendLine("    `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,");
            sb.AppendLine("    `Int2` INTEGER NULL,");
            sb.AppendLine("    `Long2` INTEGER NULL,");
            sb.AppendLine("    `Float2` REAL NULL,");
            sb.AppendLine("    `Double2` REAL NULL,");
            sb.AppendLine("    `Decimal2` TEXT NULL,");
            sb.AppendLine("    `Decimal3` TEXT NULL,");
            sb.AppendLine("    `Guid2` TEXT NULL,");
            sb.AppendLine("    `Short2` INTEGER NULL,");
            sb.AppendLine("    `DateTime2` TEXT NULL,");
            sb.AppendLine("    `Bool2` INTEGER NULL,");
            sb.AppendLine("    `TimeSpan2` TEXT NULL,");
            sb.AppendLine("    `Byte2` INTEGER NULL,");
            sb.AppendLine("    `String2` TEXT NULL,");
            sb.AppendLine("    `String3` TEXT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(0, result[0].Descriptions.Count);

            //dbGenerator.ExecuteGenerateSql(result[0]);
            sb.Clear();
            sb.AppendLine("CREATE TABLE `NotNullableTable2` (");
            sb.AppendLine("    `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,");
            sb.AppendLine("    `Int2` INTEGER NOT NULL,");
            sb.AppendLine("    `Long2` INTEGER NOT NULL,");
            sb.AppendLine("    `Float2` REAL NOT NULL,");
            sb.AppendLine("    `Double2` REAL NOT NULL,");
            sb.AppendLine("    `Decimal2` TEXT NOT NULL,");
            sb.AppendLine("    `Decimal3` TEXT NOT NULL,");
            sb.AppendLine("    `Guid2` TEXT NOT NULL,");
            sb.AppendLine("    `Short2` INTEGER NOT NULL,");
            sb.AppendLine("    `DateTime2` TEXT NOT NULL,");
            sb.AppendLine("    `Bool2` INTEGER NOT NULL,");
            sb.AppendLine("    `TimeSpan2` TEXT NOT NULL,");
            sb.AppendLine("    `Byte2` INTEGER NOT NULL,");
            sb.AppendLine("    `String2` TEXT NOT NULL,");
            sb.AppendLine("    `String3` TEXT NOT NULL");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1].Body);

            sb.Clear();
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
            Assert.Equal(1, result.Count());
            Assert.Equal(0, result[0].Descriptions.Count);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE NullableTable ADD `int3` INTEGER NULL", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE `SpecifiedMapTestTable` (");
            sb.AppendLine("    `NormalTxt` TEXT NULL,");
            sb.AppendLine("    `SpecifiedTxt` real NULL");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
        }


        [Fact, Priority(302)]
        public void TestSqlite()
        {
            InitSqliteDatabase("Data Source=./TestSqlite.db");
            TestRepository();
        }

        [Fact, Priority(301)]
        public async Task TestSqliteAsync()
        {
            InitSqliteDatabase("Data Source=./TestSqliteAsync.db");
            await TestRepositoryAsync();
        }
        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址
        private void InitServices(string databaseString)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(CONFIG_FILE, true, true);
            var configurationRoot = build.Build();
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSingleton<IConfiguration>(configurationRoot);

            if (string.IsNullOrWhiteSpace(databaseString))
            {
                throw new ArgumentNullException("sqlite connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                it.AddDatabaseUnit<SQLiteConnection, IUnitOfWork1>(databaseString,
                    x =>
                    {
                        x.BindRepositorysWithAttribute<SqliteAutoRepositoryAttribute>();
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

        private void InitSqliteDatabase(string databaseString)
        {
            //初始化数据库
            using (var database = new SqliteDb(databaseString))    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }

            InitServices(databaseString);
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

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single(age5Customers);
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
                var orderDetail3 = new OrderDetail();
                orderDetail3.OrderHeaderId = orderHeader.Id;
                orderDetail3.ProductName = "ball";
                orderDetail3.Quantity = 3;
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
            var avgResult = await customerRepository.Where(it => it.Age >= 98).AverageAsync(it => (double)it.Age);
            Assert.Equal((99 + 98) / (double)2, avgResult);

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

            //Test update 
            var newCount2 = await customerRepository.Where(it => it.Age > 5).SetValue(it => it.Name, "a")
                .ExecuteUpdateAsync();
            Assert.Equal(94, newCount2);
            //Test delete 
            var newCount3 = await customerRepository.DeleteAsync(it=>it.Age>5);
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

        public void TestRepository()
        {
            var uow = serviceProvider.GetService<IUnitOfWork1>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //Test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            customerRepository.Insert(customer);

            customerRepository.Where(it=>it.Name == "testCustomer")
                .SetValue(it=>it.Age,5)
                .SetValue(it=>it.TotalConsumptionAmount,100)
                .ExecuteUpdate();

            var age5Customers= customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single(age5Customers);
            Assert.Equal(5,age5Customers[0].Age);
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
            Assert.Equal((99 + 98) / (double)2, avgResult);

            //测试bindWhere构造条件
            var nameEmpty = WhereBuilder.Empty<string>();
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
            customerRepository.Delete(it=>it.Age>5);
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
