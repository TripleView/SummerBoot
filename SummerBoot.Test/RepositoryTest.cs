using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.WebApi.Models;
using SummerBoot.WebApi.Repository;
using Xunit;

namespace SummerBoot.WebApi
{
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;
        private void InitDatabase()
        {
            //初始化数据库
            using (var database = new Db())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }

            var services = new ServiceCollection();

            services.AddSummerBoot();

            services.AddSummerBootRepository(it =>
            {
                it.DbConnectionType = typeof(SQLiteConnection);
                it.ConnectionString = @"Data source=./testDb.db";
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        }
        private void InitAsyncDatabase()
        {
            //初始化数据库
            using (var database = new Db2())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }

            var services = new ServiceCollection();

            services.AddSummerBoot();

            services.AddSummerBootRepository(it =>
            {
                it.DbConnectionType = typeof(SQLiteConnection);
                it.ConnectionString = @"Data source=./testDbAsync.db";
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        }
        
        [Fact]
        public async Task TestRepositoryAsync()
        {
            InitAsyncDatabase();
           
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            var f = await customerRepository.GetDatetime();

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

            //test unitOfWork
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

            //test page
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer() { Age = i, Name = "page" + i });
            }

            var newCount = await customerRepository.BatchInsertAsync(customers);
            Assert.Equal(100, newCount);
            var pageable = new Pageable(1, 10);
            var page = await customerRepository.GetCustomerByPageAsync(pageable, 5);
            //0-99岁，大于5的只有94个
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);

            //test update 
            var newCount2 = await customerRepository.UpdateCustomerAsync("a", 5);
            await customerRepository.UpdateCustomerTask("b", 5);
            Assert.Equal(94, newCount2);
            //test delete 
            var newCount3 = await customerRepository.DeleteCustomerAsync( 5);
            Assert.Equal(94, newCount3);
            await customerRepository.DeleteCustomerTask( 5);
            var newCount4 = await customerRepository.GetAllAsync();
            Assert.Equal(8, newCount4.Count);
        }
        [Fact]
        public void TestRepository()
        {
            InitDatabase();

            var uow = serviceProvider.GetService<IUnitOfWork>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            //test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            customerRepository.Insert(customer);

            var orderHeader = new OrderHeader();
            orderHeader.CreateTime = DateTime.UtcNow;
            orderHeader.CustomerId = customer.Id;
            orderHeader.State = 1;
            orderHeader.OrderNo = Guid.NewGuid().ToString("N");
            orderHeaderRepository.Insert(orderHeader);

            var orderDetail = new OrderDetail();
            orderDetail.OrderHeaderId = orderHeader.Id;
            orderDetail.ProductName = "apple";
            orderDetail.Quantity = 1;
            orderDetailRepository.Insert(orderDetail);

            var orderDetail2 = new OrderDetail();
            orderDetail2.OrderHeaderId = orderHeader.Id;
            orderDetail2.ProductName = "orange";
            orderDetail2.Quantity = 2;
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

            //test unitOfWork
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
                var orderDetail4 = new OrderDetail();
                orderDetail4.OrderHeaderId = orderHeader.Id;
                orderDetail4.ProductName = "basketball";
                orderDetail4.Quantity = 4;
                orderDetailRepository.Insert(orderDetail4);
                uow.Commit();
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

            //test page
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer() { Age = i, Name = "page" + i });
            }

            var newCount = customerRepository.BatchInsert(customers);
            Assert.Equal(100, newCount);
            var pageable = new Pageable(1, 10);
            var page = customerRepository.GetCustomerByPage(pageable, 5);
            //0-99岁，大于5的只有94个
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);

            //test update 
            var newCount2 = customerRepository.UpdateCustomer("a", 5);
            customerRepository.UpdateCustomerTask("b", 5);
            Assert.Equal(94, newCount2);
            //test delete 
            var newCount3 =  customerRepository.DeleteCustomer(5);
            Assert.Equal(94, newCount3);
            customerRepository.DeleteCustomerNoReturn(5);
            var newCount4 =  customerRepository.GetAll();
            Assert.Equal(8, newCount4.Count);
        }
    }
}
