using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Oracle.Db;
using SummerBoot.Test.Oracle.Models;
using SummerBoot.Test.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.TypeHandler;
using SummerBoot.Test.Model;
using SummerBoot.Test.Oracle.Dto;
using Xunit;
using Xunit.Priority;
using StackExchange.Redis;
using SummerBoot.Test.Common.Dto;

namespace SummerBoot.Test.Oracle
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        [Fact, Priority(226)]
        public async Task TestStructOrClassAsParameter()
        {
            //InitDatabase();
            InitService();
            var guid = Guid.NewGuid();
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var testStructAsParametersOrReturnValuesRepository = serviceProvider.GetService<ITestStructAsParametersOrReturnValuesRepository>();
            var guidModelForListParameterRepository = serviceProvider.GetService<IGuidModelForListParameterRepository>();

            var model = new GuidModel()
            {
                Id = guid,
                Name = "a"
            };

            await guidModelRepository.InsertAsync(model);
            
            var r3 = await guidModelForListParameterRepository.TestStructAsParameter(new TestReturnStruct()
            {
                Name = "a"
            });
        }

        [Fact, Priority(225)]
        public async Task TestStructOrClassAsReturnValue()
        {
            InitDatabase();
            //InitService();
            var guid = Guid.NewGuid();
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var testStructAsParametersOrReturnValuesRepository = serviceProvider.GetService<ITestStructAsParametersOrReturnValuesRepository>();
            
            var model = new GuidModel()
            {
                Id = guid,
                Name = "a"
            };

            await guidModelRepository.InsertAsync(model);
            var r = await testStructAsParametersOrReturnValuesRepository.TestReturnClass();
            var r2 = await testStructAsParametersOrReturnValuesRepository.TestReturnStruct();
            Assert.True(r.Count==r2.Count);
            for (var i = 0; i < r.Count; i++)
            {
                var item1 = r[i];
                var item2 = r2[i];
                Assert.Equal(item1.Name,item2.Name);
                Assert.Equal(item1.Guid, item2.Guid);
            }

        }


        [Fact, Priority(224)]
        public async Task TestAnonymousListsAsParameters()
        {
            InitDatabase();
            var guid = Guid.NewGuid();
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var guidModelForListParameterRepository = serviceProvider.GetService<IGuidModelForListParameterRepository>();

            var model = new GuidModel()
            {
                Name = "a"
            };
            
            await guidModelRepository.InsertAsync(model);
            var r = await guidModelForListParameterRepository.GetListByNames(new List<string>(){"a","b","c"});
            Assert.Equal(1, r.Count);
            Assert.Equal("a", r[0].Name);
        }

        [Fact, Priority(223)]
        public async Task TestGuid()
        {
            InitDatabase();
            var guid = Guid.NewGuid();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var nullableTableManualRepository = serviceProvider.GetService<INullableTableManualRepository>();

            var model = new NullableTable()
            {
                Guid2 = guid
            };
            await nullableTableRepository.InsertAsync(model);
            var r = await nullableTableManualRepository.GetGuid();
            Assert.Equal(guid, r);
            var r2 = await nullableTableManualRepository.GetGuidWithNullResult();
            Assert.Equal(null, null);
        }

        /// <summary>
        /// ����left/right joinʱ����ʵ��������string����,����string����Ϊnullʱ����trim����
        /// </summary>
        [Fact, Priority(222)]
        public async Task TestJoinEntityWithStringNullCallMethod()
        {
            InitDatabase();

            var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
            var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
            var propNullTest = new PropNullTest()
            {
                Name = "test"
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

            var result = await propNullTestRepository.InnerJoin(new PropNullTestItem(), it => it.T1.Id == it.T2.MapId && it.T2.Name == test.Name.Trim())
                .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
            Assert.Empty(result);

        }


        /// <summary>
        /// ����left/right joinʱ����ʵ��������������Ϊ�ɿ����� 
        /// </summary>
        [Fact, Priority(221)]
        public async Task TestJoinEntityWithNullableProperty()
        {
            InitDatabase();

            var propNullTestRepository = serviceProvider.GetService<IPropNullTestRepository>();
            var propNullTestItemRepository = serviceProvider.GetService<IPropNullTestItemRepository>();
            var propNullTest = new PropNullTest()
            {
                Name = "test"
            };
            await propNullTestRepository.InsertAsync(propNullTest);
            var propNullTestItem = new PropNullTestItem()
            {
                Name = "testitem",
                MapId = propNullTest.Id
            };
            await propNullTestItemRepository.InsertAsync(propNullTestItem);

            var result = await propNullTestRepository.LeftJoin(new PropNullTestItem(), it => it.T1.Id == it.T2.MapId)
                .Select(it => new { it.T1.Name, it.T2.MapId }).ToListAsync();
            Assert.Single(result);
            Assert.Equal("test", result.First().Name);
            Assert.Equal(1, result.First().MapId);
        }

        /// <summary>
        /// ����where�����в�����������
        /// </summary>
        [Fact, Priority(220)]
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
        /// ����where�����в����ǿɿ�����
        /// </summary>
        [Fact, Priority(219)]
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
        /// ����where�����ĸ������
        /// </summary>
        [Fact, Priority(218)]
        public async Task TestWhereCombine()
        {
            InitDatabase();

            var addressRepository = serviceProvider.GetService<IAddressRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var birthDay = new DateTime(1992, 2, 15);
            var customer1 = new Customer()
            {
                Age = 1,
                Name = "bob",
                CustomerNo = "A1",
                TotalConsumptionAmount = 1,
                BirthDay = birthDay
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

            #region ���Ե���where

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

            var newTestCustomer = new Customer()
            {
                CustomerNo = " A2 "
            };
            var customerList9 = await customerRepository.Where(it => it.CustomerNo.Contains(newTestCustomer.CustomerNo.Trim())).ToListAsync();
            Assert.Equal(1, customerList9.Count);
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
            #region ����˫��where

            var result01 = await customerRepository
                .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId && it.T2.City == new { city = "B" }.city)
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(1, result01.Count);

            var result02 = await customerRepository
                .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T2.City == new { city = "B" }.city)
                .Select(it => new { it.T1.CustomerNo, it.T2.City })
                .ToListAsync();
            Assert.Equal(1, result02.Count);

            var result03 = await customerRepository
                .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T2.City == new { city = "B" }.city)
                .Select(it => it.T1.CustomerNo)
                .ToListAsync();
            Assert.Equal(1, result03.Count);

            var result04 = await customerRepository
                .InnerJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
                .Where(it => it.T2.City == new { city = "B" }.city)
                .Select(it => new { it.T1.BirthDay, it.T2.City })
                .ToListAsync();
            Assert.Equal(1, result04.Count);
            Assert.Equal(birthDay, result04[0].BirthDay);

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

            #region ����3��where
            //���Զ�join��where����
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

            #region ����4��where
            //���Զ�join��where����
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
        /// ����where�������ұߵ�ֵΪdto�����ԣ�����ͬʱ���Դ�list���ȡ����Ϊ0����1��ֵ
        /// </summary>
        [Fact, Priority(218)]
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
        /// <summary>
        /// ����3�ű�����
        /// </summary>
        [Fact, Priority(217)]
        public async Task TestLeftJoin4TableAsync()
        {
            InitDatabase();
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

            ////���Է�ҳ
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
        /// ����3�ű�����
        /// </summary>
        [Fact, Priority(216)]
        public async Task TestLeftJoin3TableAsync()
        {
            InitDatabase();
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

            ////���Է�ҳ
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
        /// ����2�ű�����
        /// </summary>
        [Fact, Priority(215)]
        public async Task TestLeftJoin2TableAsync()
        {
            InitDatabase();
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

            //���Է�ҳ
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
        /// ����3�ű�����
        /// </summary>
        [Fact, Priority(217)]
        public async Task TestLeftJoin4Table()
        {
            InitDatabase();
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

            ////���Է�ҳ
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
        /// ����3�ű�����
        /// </summary>
        [Fact, Priority(216)]
        public async Task TestLeftJoin3Table()
        {
            InitDatabase();
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

            ////���Է�ҳ
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
        /// ����2�ű�����
        /// </summary>
        [Fact, Priority(215)]
        public async Task TestLeftJoin2Table()
        {
            InitDatabase();
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

            //���Է�ҳ
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
        /// ���Բ���ʵ��͸���ʵ��ǰ���Զ��庯��
        /// </summary>
        [Fact, Priority(113)]
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
        /// ����id����Ϊguid��model����ɾ�Ĳ�
        /// </summary>
        [Fact, Priority(214)]
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
        /// ������������������
        /// </summary>
        [Fact, Priority(213)]
        public async Task TestBatchInsertWithDbtransation()
        {
            InitDatabase();

            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();

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
        /// ������������
        /// </summary>
        [Fact, Priority(212)]
        public async Task TestBatchInsertAsync()
        {
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();
            sw.Start();
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
                    Enum2 = Model.Enum2.y
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;

            sw.Restart();
            await nullableTableRepository.InsertAsync(nullableTableList);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;

            sw.Restart();
            //Customer
            var connection = dbFactory.GetDbConnection() as OracleConnection;
            //connection.Open();
            int?[] Int2 = new int?[total];
            bool[] Bool2 = new bool[total];
            byte[] Byte2 = new byte[total];
            DateTime[] DateTime2 = new DateTime[total];
            decimal?[] Decimal2 = new decimal?[total];
            decimal[] Decimal3 = new decimal[total];
            double[] Double2 = new double[total];
            float[] Float2 = new float[total];
            Guid?[] Guid2 = new Guid?[total];
            short[] Short2 = new short[total];
            TimeSpan[] TimeSpan2 = new TimeSpan[total];
            string[] String2 = new string[total];
            string[] String3 = new string[total];
            long[] Long2 = new long[total];
            Enum2[] Enum2 = new Enum2[total];

            for (int j = 0; j < total; j++)
            {
                Int2[j] = 2;
                Bool2[j] = true;
                Byte2[j] = 1;
                DateTime2[j] = now2;
                Decimal2[j] = 1m;
                Decimal3[j] = 1.1m;
                Double2[j] = 1.1;
                Float2[j] = (float)1.1;
                Guid2[j] = Guid.NewGuid();
                Short2[j] = 1;
                TimeSpan2[j] = TimeSpan.FromHours(1);
                String2[j] = "sb";
                String3[j] = "sb";
                Long2[j] = 2;
                Enum2[j] = Model.Enum2.y;
                if (j == 0)
                {
                    Guid2[j] = guid;
                }
            }

            var c = (int)Model.Enum2.y;
            OracleParameter pInt2 = new OracleParameter();
            pInt2.OracleDbType = OracleDbType.Int32;
            pInt2.Value = Int2;

            OracleParameter pBool2 = new OracleParameter();
            pBool2.OracleDbType = OracleDbType.Byte;
            pBool2.Value = Bool2;

            OracleParameter pByte2 = new OracleParameter();
            pByte2.OracleDbType = OracleDbType.Byte;
            pByte2.Value = Byte2;

            OracleParameter pDateTime2 = new OracleParameter();
            pDateTime2.OracleDbType = OracleDbType.TimeStamp;
            pDateTime2.Value = DateTime2;

            OracleParameter pDecimal2 = new OracleParameter();
            pDecimal2.OracleDbType = OracleDbType.Decimal;
            pDecimal2.Value = Decimal2;

            OracleParameter pDecimal3 = new OracleParameter();
            pDecimal3.OracleDbType = OracleDbType.Decimal;
            pDecimal3.Value = Decimal3;

            OracleParameter pDouble2 = new OracleParameter();
            pDouble2.OracleDbType = OracleDbType.Double;
            pDouble2.Value = Double2;

            OracleParameter pFloat2 = new OracleParameter();
            pFloat2.OracleDbType = OracleDbType.BinaryFloat;
            pFloat2.Value = Float2;


            OracleParameter pGuid2 = new OracleParameter();
            pGuid2.OracleDbType = OracleDbType.Raw;
            pGuid2.Value = Guid2;

            OracleParameter pShort2 = new OracleParameter();
            pShort2.OracleDbType = OracleDbType.Int16;
            pShort2.Value = Short2;

            OracleParameter pTimeSpan2 = new OracleParameter();
            pTimeSpan2.OracleDbType = OracleDbType.IntervalDS;
            pTimeSpan2.Value = TimeSpan2;

            OracleParameter pString2 = new OracleParameter();
            pString2.OracleDbType = OracleDbType.Varchar2;
            pString2.Value = String2;

            OracleParameter pString3 = new OracleParameter();
            pString3.OracleDbType = OracleDbType.Varchar2;
            pString3.Value = String3;


            OracleParameter pLong2 = new OracleParameter();
            pLong2.OracleDbType = OracleDbType.Long;
            pLong2.Value = Long2;

            OracleParameter pEnum2 = new OracleParameter();
            pEnum2.OracleDbType = OracleDbType.Byte;
            pEnum2.Value = Enum2;
            // create command and set properties
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO NULLABLETABLE (INT2, LONG2, FLOAT2, DOUBLE2, DECIMAL2, DECIMAL3, GUID2, SHORT2, DATETIME2, BOOL2, TIMESPAN2, BYTE2, STRING2, STRING3,ENUM2) VALUES(:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15)";
            cmd.ArrayBindCount = total;
            cmd.Parameters.Add(pInt2);
            cmd.Parameters.Add(pLong2);
            cmd.Parameters.Add(pFloat2);
            cmd.Parameters.Add(pDouble2);
            cmd.Parameters.Add(pDecimal2);
            cmd.Parameters.Add(pDecimal3);
            cmd.Parameters.Add(pGuid2);
            cmd.Parameters.Add(pShort2);
            cmd.Parameters.Add(pDateTime2);
            cmd.Parameters.Add(pBool2);
            cmd.Parameters.Add(pTimeSpan2);
            cmd.Parameters.Add(pByte2);
            cmd.Parameters.Add(pString2);
            cmd.Parameters.Add(pString3);
            cmd.Parameters.Add(pEnum2);
            await cmd.ExecuteNonQueryAsync();
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            var count = nullableTableRepository.Count(it => it.Guid2 == guid);
            Assert.Equal(3, count);
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
        }

        /// <summary>
        /// ������������
        /// </summary>
        [Fact, Priority(211)]
        public async Task TestBatchInsert()
        {
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();
            sw.Start();
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
                    Enum2 = Model.Enum2.y
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            nullableTableRepository.FastBatchInsert(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;

            sw.Restart();
            nullableTableRepository.Insert(nullableTableList);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;

            sw.Restart();
            //Customer
            var connection = dbFactory.GetDbConnection() as OracleConnection;
            //connection.Open();
            int?[] Int2 = new int?[total];
            bool[] Bool2 = new bool[total];
            byte[] Byte2 = new byte[total];
            DateTime[] DateTime2 = new DateTime[total];
            decimal?[] Decimal2 = new decimal?[total];
            decimal[] Decimal3 = new decimal[total];
            double[] Double2 = new double[total];
            float[] Float2 = new float[total];
            Guid?[] Guid2 = new Guid?[total];
            short[] Short2 = new short[total];
            TimeSpan[] TimeSpan2 = new TimeSpan[total];
            string[] String2 = new string[total];
            string[] String3 = new string[total];
            long[] Long2 = new long[total];
            Enum2[] Enum2 = new Enum2[total];

            for (int j = 0; j < total; j++)
            {
                Int2[j] = 2;
                Bool2[j] = true;
                Byte2[j] = 1;
                DateTime2[j] = now2;
                Decimal2[j] = 1m;
                Decimal3[j] = 1.1m;
                Double2[j] = 1.1;
                Float2[j] = (float)1.1;
                Guid2[j] = Guid.NewGuid();
                Short2[j] = 1;
                TimeSpan2[j] = TimeSpan.FromHours(1);
                String2[j] = "sb";
                String3[j] = "sb";
                Long2[j] = 2;
                Enum2[j] = Model.Enum2.y;
                if (j == 0)
                {
                    Guid2[j] = guid;
                }
            }

            OracleParameter pInt2 = new OracleParameter();
            pInt2.OracleDbType = OracleDbType.Int32;
            pInt2.Value = Int2;

            OracleParameter pBool2 = new OracleParameter();
            pBool2.OracleDbType = OracleDbType.Byte;
            pBool2.Value = Bool2;

            OracleParameter pByte2 = new OracleParameter();
            pByte2.OracleDbType = OracleDbType.Byte;
            pByte2.Value = Byte2;

            OracleParameter pDateTime2 = new OracleParameter();
            pDateTime2.OracleDbType = OracleDbType.TimeStamp;
            pDateTime2.Value = DateTime2;

            OracleParameter pDecimal2 = new OracleParameter();
            pDecimal2.OracleDbType = OracleDbType.Decimal;
            pDecimal2.Value = Decimal2;

            OracleParameter pDecimal3 = new OracleParameter();
            pDecimal3.OracleDbType = OracleDbType.Decimal;
            pDecimal3.Value = Decimal3;

            OracleParameter pDouble2 = new OracleParameter();
            pDouble2.OracleDbType = OracleDbType.Double;
            pDouble2.Value = Double2;

            OracleParameter pFloat2 = new OracleParameter();
            pFloat2.OracleDbType = OracleDbType.BinaryFloat;
            pFloat2.Value = Float2;


            OracleParameter pGuid2 = new OracleParameter();
            pGuid2.OracleDbType = OracleDbType.Raw;
            pGuid2.Value = Guid2;

            OracleParameter pShort2 = new OracleParameter();
            pShort2.OracleDbType = OracleDbType.Int16;
            pShort2.Value = Short2;

            OracleParameter pTimeSpan2 = new OracleParameter();
            pTimeSpan2.OracleDbType = OracleDbType.IntervalDS;
            pTimeSpan2.Value = TimeSpan2;

            OracleParameter pString2 = new OracleParameter();
            pString2.OracleDbType = OracleDbType.Varchar2;
            pString2.Value = String2;

            OracleParameter pString3 = new OracleParameter();
            pString3.OracleDbType = OracleDbType.Varchar2;
            pString3.Value = String3;


            OracleParameter pLong2 = new OracleParameter();
            pLong2.OracleDbType = OracleDbType.Long;
            pLong2.Value = Long2;

            OracleParameter pEnum2 = new OracleParameter();
            pEnum2.OracleDbType = OracleDbType.Byte;
            pEnum2.Value = Enum2;
            // create command and set properties
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO NULLABLETABLE (INT2, LONG2, FLOAT2, DOUBLE2, DECIMAL2, DECIMAL3, GUID2, SHORT2, DATETIME2, BOOL2, TIMESPAN2, BYTE2, STRING2, STRING3,ENUM2) VALUES(:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15)";
            cmd.ArrayBindCount = total;
            cmd.Parameters.Add(pInt2);
            cmd.Parameters.Add(pLong2);
            cmd.Parameters.Add(pFloat2);
            cmd.Parameters.Add(pDouble2);
            cmd.Parameters.Add(pDecimal2);
            cmd.Parameters.Add(pDecimal3);
            cmd.Parameters.Add(pGuid2);
            cmd.Parameters.Add(pShort2);
            cmd.Parameters.Add(pDateTime2);
            cmd.Parameters.Add(pBool2);
            cmd.Parameters.Add(pTimeSpan2);
            cmd.Parameters.Add(pByte2);
            cmd.Parameters.Add(pString2);
            cmd.Parameters.Add(pString3);
            cmd.Parameters.Add(pEnum2);
            cmd.ExecuteNonQuery();
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            var count = nullableTableRepository.Count(it => it.Guid2 == guid);
            Assert.Equal(3, count);
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
        }

        /// <summary>
        /// ���Դ������ļ���ȡsql
        /// </summary>
        [Fact, Priority(210)]
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
        /// ���Դ������ļ���ȡsql
        /// </summary>
        [Fact, Priority(209)]
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
        /// ���Դ������ռ���������������
        /// </summary>
        [Fact, Priority(208)]
        public void TestTableSchemaAndAddPrimaryKey()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var customerWithSchema2Repository = serviceProvider.GetService<ICustomerWithSchema2Repository>();
            var sb = new StringBuilder();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema) });
            sb.Clear();
            sb.AppendLine("CREATE TABLE TEST1.\"CUSTOMERWITHSCHEMA\" (");
            sb.AppendLine("    \"NAME\" NVARCHAR2(2000),");
            sb.AppendLine("    \"AGE\" NUMBER(10,0) NOT NULL,");
            sb.AppendLine("    \"CUSTOMERNO\" NVARCHAR2(2000),");
            sb.AppendLine("    \"TOTALCONSUMPTIONAMOUNT\" NUMBER(18,2) NOT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema2) });
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"ID\" NUMBER(10,0) GENERATED BY DEFAULT ON NULL AS IDENTITY MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE  NOKEEP  NOSCALE NOT NULL"
                , result[0].FieldModifySqls[0]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD CONSTRAINT CUSTOMERWITHSCHEMA_PK PRIMARY KEY(ID) ENABLE"
                , result[0].FieldModifySqls[1]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"LASTUPDATEON\" TIMESTAMP(7)"
                , result[0].FieldModifySqls[2]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"LASTUPDATEBY\" NVARCHAR2(2000)"
                , result[0].FieldModifySqls[3]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"CREATEON\" TIMESTAMP(7)"
                , result[0].FieldModifySqls[4]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"CREATEBY\" NVARCHAR2(2000)"
                , result[0].FieldModifySqls[5]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"ACTIVE\" NUMBER(10,0)"
                , result[0].FieldModifySqls[6]);
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
        /// ????????????????????????��?????????
        /// </summary>
        [Fact, Priority(207)]
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
        /// ??????????
        /// </summary>
        [Fact, Priority(206)]
        public void TestTypeHandler()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var testTypeHandlerTableRepository = serviceProvider.GetService<ITestTypeHandlerTableRepository>();
            var sqls = dbGenerator.GenerateSql(new List<Type>() { typeof(TestTypeHandlerTable) });
            foreach (var generateDatabaseSqlResult in sqls)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            Guid guid = Guid.NewGuid();

            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = true, Name = "true", NullableBoolColumn = true, GuidColumn = guid, NullableGuidColumn = guid });
            var testTypeHandlerTableTrue = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "true");
            Assert.Equal(true, testTypeHandlerTableTrue.BoolColumn);
            Assert.Equal(true, testTypeHandlerTableTrue.NullableBoolColumn);
            Assert.Equal(guid, testTypeHandlerTableTrue.GuidColumn);
            Assert.Equal(guid, testTypeHandlerTableTrue.NullableGuidColumn);
            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = false, Name = "false", NullableBoolColumn = false });
            var testTypeHandlerTableFalse = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "false");
            Assert.Equal(false, testTypeHandlerTableFalse.BoolColumn);
            Assert.Equal(false, testTypeHandlerTableFalse.NullableBoolColumn);
            Assert.Equal(Guid.Empty, testTypeHandlerTableFalse.GuidColumn);
            Assert.Equal(null, testTypeHandlerTableFalse.NullableGuidColumn);
            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = false, Name = "falseAndNull", NullableBoolColumn = null, GuidColumn = guid, NullableGuidColumn = null });
            var testTypeHandlerTableFalseAndNull = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "falseAndNull");
            Assert.Equal(false, testTypeHandlerTableFalseAndNull.BoolColumn);
            Assert.Equal(null, testTypeHandlerTableFalseAndNull.NullableBoolColumn);
            Assert.Equal(guid, testTypeHandlerTableFalseAndNull.GuidColumn);
            Assert.Equal(null, testTypeHandlerTableFalseAndNull.NullableGuidColumn);
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        [Fact, Priority(205)]
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

        [Fact, Priority(204)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateCsharpClass(new List<string>() { "CUSTOMER", "NULLABLETABLE", "NOTNULLABLETABLE" }, "abc");
            Assert.Equal(3, result.Count);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("namespace abc");
            sb.AppendLine("{");
            sb.AppendLine("   [Table(\"CUSTOMER\")]");
            sb.AppendLine("   public class CUSTOMER");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [Column(\"ID\")]");
            sb.AppendLine("      public int ID { get; set; }");
            sb.AppendLine("      [Column(\"NAME\")]");
            sb.AppendLine("      public string NAME { get; set; }");
            sb.AppendLine("      [Column(\"AGE\")]");
            sb.AppendLine("      public int AGE { get; set; }");
            sb.AppendLine("      [Column(\"CUSTOMERNO\")]");
            sb.AppendLine("      public string CUSTOMERNO { get; set; }");
            sb.AppendLine("      [Column(\"TOTALCONSUMPTIONAMOUNT\")]");
            sb.AppendLine("      public decimal TOTALCONSUMPTIONAMOUNT { get; set; }");
            sb.AppendLine("      [Column(\"BIRTHDAY\")]");
            sb.AppendLine("      public DateTime? BIRTHDAY { get; set; }");
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
            sb.AppendLine("   [Table(\"NULLABLETABLE\")]");
            sb.AppendLine("   public class NULLABLETABLE");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [Column(\"ID\")]");
            sb.AppendLine("      public int ID { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"INT2\")]");
            sb.AppendLine("      public int? INT2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"LONG2\")]");
            sb.AppendLine("      public long? LONG2 { get; set; }");
            sb.AppendLine("      [Column(\"FLOAT2\")]");
            sb.AppendLine("      public float? FLOAT2 { get; set; }");
            sb.AppendLine("      [Column(\"DOUBLE2\")]");
            sb.AppendLine("      public double? DOUBLE2 { get; set; }");
            sb.AppendLine("      [Column(\"DECIMAL2\")]");
            sb.AppendLine("      public decimal? DECIMAL2 { get; set; }");
            sb.AppendLine("      [Column(\"DECIMAL3\")]");
            sb.AppendLine("      public decimal? DECIMAL3 { get; set; }");
            sb.AppendLine("      [Column(\"GUID2\")]");
            sb.AppendLine("      public Guid? GUID2 { get; set; }");
            sb.AppendLine("      [Column(\"SHORT2\")]");
            sb.AppendLine("      public short? SHORT2 { get; set; }");
            sb.AppendLine("      [Column(\"DATETIME2\")]");
            sb.AppendLine("      public DateTime? DATETIME2 { get; set; }");
            sb.AppendLine("      [Column(\"BOOL2\")]");
            sb.AppendLine("      public bool? BOOL2 { get; set; }");
            sb.AppendLine("      [Column(\"TIMESPAN2\")]");
            sb.AppendLine("      public TimeSpan? TIMESPAN2 { get; set; }");
            sb.AppendLine("      [Column(\"BYTE2\")]");
            sb.AppendLine("      public byte? BYTE2 { get; set; }");
            sb.AppendLine("      [Column(\"STRING2\")]");
            sb.AppendLine("      public string STRING2 { get; set; }");
            sb.AppendLine("      [Column(\"STRING3\")]");
            sb.AppendLine("      public string STRING3 { get; set; }");
            sb.AppendLine("      [Column(\"ENUM2\")]");
            sb.AppendLine("      public int? ENUM2 { get; set; }");
            sb.AppendLine("      [Column(\"TESTINT3\")]");
            sb.AppendLine("      public int? TESTINT3 { get; set; }");
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
            sb.AppendLine("   [Table(\"NOTNULLABLETABLE\")]");
            sb.AppendLine("   public class NOTNULLABLETABLE");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [Column(\"ID\")]");
            sb.AppendLine("      public int ID { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"INT2\")]");
            sb.AppendLine("      public int INT2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"LONG2\")]");
            sb.AppendLine("      public long LONG2 { get; set; }");
            sb.AppendLine("      [Column(\"FLOAT2\")]");
            sb.AppendLine("      public float FLOAT2 { get; set; }");
            sb.AppendLine("      [Column(\"DOUBLE2\")]");
            sb.AppendLine("      public double DOUBLE2 { get; set; }");
            sb.AppendLine("      [Column(\"DECIMAL2\")]");
            sb.AppendLine("      public decimal DECIMAL2 { get; set; }");
            sb.AppendLine("      [Column(\"DECIMAL3\")]");
            sb.AppendLine("      public decimal DECIMAL3 { get; set; }");
            sb.AppendLine("      [Column(\"GUID2\")]");
            sb.AppendLine("      public Guid GUID2 { get; set; }");
            sb.AppendLine("      [Column(\"SHORT2\")]");
            sb.AppendLine("      public short SHORT2 { get; set; }");
            sb.AppendLine("      [Column(\"DATETIME2\")]");
            sb.AppendLine("      public DateTime DATETIME2 { get; set; }");
            sb.AppendLine("      [Column(\"BOOL2\")]");
            sb.AppendLine("      public bool BOOL2 { get; set; }");
            sb.AppendLine("      [Column(\"TIMESPAN2\")]");
            sb.AppendLine("      public TimeSpan TIMESPAN2 { get; set; }");
            sb.AppendLine("      [Column(\"BYTE2\")]");
            sb.AppendLine("      public byte BYTE2 { get; set; }");
            sb.AppendLine("      [Column(\"STRING2\")]");
            sb.AppendLine("      public string STRING2 { get; set; }");
            sb.AppendLine("      [Column(\"STRING3\")]");
            sb.AppendLine("      public string STRING3 { get; set; }");
            sb.AppendLine("   }");
            sb.AppendLine("}");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[2]);
        }

        /// <summary>
        /// ???????c#????????????
        /// </summary>
        [Fact, Priority(203)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE TEST.\"NULLABLETABLE2\" (");
            sb.AppendLine("    \"ID\" NUMBER(10,0) GENERATED BY DEFAULT ON NULL AS IDENTITY MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE  NOKEEP  NOSCALE NOT NULL,");
            sb.AppendLine("    \"INT2\" NUMBER(10,0),");
            sb.AppendLine("    \"LONG2\" NUMBER(19,0),");
            sb.AppendLine("    \"FLOAT2\" BINARY_FLOAT,");
            sb.AppendLine("    \"DOUBLE2\" BINARY_DOUBLE,");
            sb.AppendLine("    \"DECIMAL2\" NUMBER(18,2),");
            sb.AppendLine("    \"DECIMAL3\" NUMBER(20,4),");
            sb.AppendLine("    \"GUID2\" RAW(16),");
            sb.AppendLine("    \"SHORT2\" NUMBER(5,0),");
            sb.AppendLine("    \"DATETIME2\" TIMESTAMP(7),");
            sb.AppendLine("    \"BOOL2\" NUMBER(1,0),");
            sb.AppendLine("    \"TIMESPAN2\" INTERVAL DAY(8) TO SECOND(7),");
            sb.AppendLine("    \"BYTE2\" NUMBER(3,0),");
            sb.AppendLine("    \"STRING2\" NVARCHAR2(100),");
            sb.AppendLine("    \"STRING3\" NVARCHAR2(2000),");
            sb.AppendLine("    \"ENUM2\" NUMBER(18,2),");
            sb.AppendLine("    \"TESTINT3\" NUMBER(10,0),");
            sb.AppendLine("    CONSTRAINT \"PK_NULLABLETABLE2\" PRIMARY KEY (\"ID\")");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(4, result[0].Descriptions.Count);
            Assert.Equal("COMMENT ON TABLE TEST.\"NULLABLETABLE2\" IS 'NullableTable2'", result[0].Descriptions[0]);
            Assert.Equal("COMMENT ON COLUMN TEST.\"NULLABLETABLE2\".\"INT2\" IS 'Int2'", result[0].Descriptions[1]);
            Assert.Equal("COMMENT ON COLUMN TEST.\"NULLABLETABLE2\".\"LONG2\" IS 'Long2'", result[0].Descriptions[2]);
            Assert.Equal("COMMENT ON COLUMN TEST.\"NULLABLETABLE2\".\"TESTINT3\" IS 'Int2'", result[0].Descriptions[3]);
            //dbGenerator.ExecuteGenerateSql(result[0]);

            sb.Clear();
            sb.AppendLine("CREATE TABLE TEST.\"NOTNULLABLETABLE2\" (");
            sb.AppendLine("    \"ID\" NUMBER(10,0) GENERATED BY DEFAULT ON NULL AS IDENTITY MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE  NOKEEP  NOSCALE NOT NULL,");
            sb.AppendLine("    \"INT2\" NUMBER(10,0) NOT NULL,");
            sb.AppendLine("    \"LONG2\" NUMBER(19,0) NOT NULL,");
            sb.AppendLine("    \"FLOAT2\" BINARY_FLOAT NOT NULL,");
            sb.AppendLine("    \"DOUBLE2\" BINARY_DOUBLE NOT NULL,");
            sb.AppendLine("    \"DECIMAL2\" NUMBER(18,2) NOT NULL,");
            sb.AppendLine("    \"DECIMAL3\" NUMBER(20,4) NOT NULL,");
            sb.AppendLine("    \"GUID2\" RAW(16) NOT NULL,");
            sb.AppendLine("    \"SHORT2\" NUMBER(5,0) NOT NULL,");
            sb.AppendLine("    \"DATETIME2\" TIMESTAMP(7) NOT NULL,");
            sb.AppendLine("    \"BOOL2\" NUMBER(1,0) NOT NULL,");
            sb.AppendLine("    \"TIMESPAN2\" INTERVAL DAY(8) TO SECOND(7) NOT NULL,");
            sb.AppendLine("    \"BYTE2\" NUMBER(3,0) NOT NULL,");
            sb.AppendLine("    \"STRING2\" NVARCHAR2(100) NOT NULL,");
            sb.AppendLine("    \"STRING3\" NVARCHAR2(2000) NOT NULL,");
            sb.AppendLine("    CONSTRAINT \"PK_NOTNULLABLETABLE2\" PRIMARY KEY (\"ID\")");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1].Body);

            sb.Clear();
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result[0].Descriptions.Count);
            Assert.Equal("COMMENT ON COLUMN TEST.\"NULLABLETABLE\".\"INT3\" IS 'test add column'", result[0].Descriptions[0]);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE TEST.\"NULLABLETABLE\" ADD \"INT3\" NUMBER(10,0)", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE TEST.\"SPECIFIEDMAPTESTTABLE\" (");
            sb.AppendLine("    \"NORMALTXT\" NVARCHAR2(2000),");
            sb.AppendLine("    \"SPECIFIEDTXT\" CLOB");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
        }

        private void InitDatabase()
        {
            //??????????
            using (var database = new OracleDb())    //????
            {
                database.Database.EnsureDeleted();
                //Thread.Sleep(2000);
                database.Database.EnsureCreated();
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON TABLE NULLABLETABLE IS 'NullableTable'");
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON COLUMN NULLABLETABLE.INT2 IS 'Int2'");
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON COLUMN NULLABLETABLE.LONG2 IS 'Long2'");
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON TABLE NotNullableTable IS 'NotNullableTable'");
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON COLUMN NotNullableTable.INT2 IS 'Int2'");
                database.Database.ExecuteSqlRaw(
                    "COMMENT ON COLUMN NotNullableTable.LONG2 IS 'Long2'");
                try
                {
                    database.Database.ExecuteSqlRaw(
                        "drop TABLE TEST1.\"CUSTOMERWITHSCHEMA\"");
                }
                catch (Exception e)
                {

                }


            }

            InitService();
        }

        [Fact, Priority(202)]
        public void TestOracle()
        {
            InitDatabase();
            TestRepository();
        }

        [Fact, Priority(201)]
        public async Task TestOracleAsync()
        {
            InitDatabase();
            await TestRepositoryAsync();
        }

        static readonly string CONFIG_FILE = "app.json";  // �����ļ���ַ
        private void InitService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // ��ȡ��ǰ����ִ��Ŀ¼
            build.AddJsonFile(CONFIG_FILE, true, true);
            var configurationRoot = build.Build();
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSingleton<IConfiguration>(configurationRoot);

            var connectionString = MyConfiguration.GetConfiguration("oracleDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("oracle connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                it.AddDatabaseUnit<OracleConnection, IUnitOfWork1>(connectionString,
                    x =>
                    {
                        x.TableNameMapping = a => a.ToUpper();
                        x.ColumnNameMapping = a => a.ToUpper();
                        x.BindRepositoriesWithAttribute<OracleAutoRepositoryAttribute>();
                        x.BindManualRepositoriesWithAttribute<OracleManualRepositoryAttribute>();
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
            //0-99??????5?????94??
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

            //????bindWhere????????
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

            //���Կ��б�
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


            customerRepository.Where(it => it.Name == "testCustomer")
                .SetValue(it => it.Age, 5)
                .SetValue(it => it.TotalConsumptionAmount, 100)
                .ExecuteUpdate();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single(age5Customers);
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
            //0-99??????5?????94??
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

            //????bindWhere????????
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

            //���Կ��б�
            var parameters = new List<string>();
            var emptyCustomers = customerRepository.Where(it => parameters.Contains(it.Name)).ToList();
            Assert.Empty(emptyCustomers);
        }

        public void TestLinq()
        {
            var uow = serviceProvider.GetService<IUnitOfWork>();
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
            var uow = serviceProvider.GetService<IUnitOfWork>();
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

        /// <summary>
        /// ����left/right joinʱ����ʵ��������string����,����string����Ϊnullʱ����trim����
        /// </summary>
       

    }
}
