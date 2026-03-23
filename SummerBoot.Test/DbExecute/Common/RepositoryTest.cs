using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using SqlParser.Net;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Common;
using SummerBoot.Test.DbExecute.Common.Db;
using SummerBoot.Test.DbExecute.Common.Models;
using SummerBoot.Test.DbExecute.Common.Repository;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.DbExecute.Common
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        private void InitMysqlDatabase()
        {
            //初始化数据库
            using (var database = new MysqlDb())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
                database.Database.ExecuteSqlRaw("set global local_infile=1");
                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE nullabletable COMMENT = 'NullableTable'");
                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE nullabletable MODIFY COLUMN `Int2` int NULL COMMENT 'Int2'");
                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE nullabletable MODIFY COLUMN `Long2` bigint NULL COMMENT 'Long2'");

                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE notnullabletable COMMENT = 'NotNullableTable'");
                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE notnullabletable MODIFY COLUMN `Int2` int not NULL COMMENT 'Int2'");
                database.Database.ExecuteSqlRaw(
                    "ALTER TABLE notnullabletable MODIFY COLUMN `Long2` bigint not NULL COMMENT 'Long2'");
                try
                {
                    database.Database.ExecuteSqlRaw(
                        "drop TABLE test1.`CustomerWithSchema`");
                }
                catch (Exception e)
                {

                }
            }
        }

        private void InitMysqlService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
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
                        x.BindRepositoriesWithAttribute<AutoRepositoryAttribute>();
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

        private void InitPgsqlDatabase()
        {
            //初始化数据库
            using (var database = new PgsqlDb())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
                database.Database.ExecuteSqlRaw(
                    "create schema test1");
            }
        }

        private void InitPgsqlService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
            var configurationRoot = build.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configurationRoot);
            services.AddSummerBoot();
            var connectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("pgsql connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                it.AddDatabaseUnit<NpgsqlConnection, IUnitOfWork1>(connectionString,
                    x =>
                    {
                        x.BindRepositoriesWithAttribute<AutoRepositoryAttribute>();
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

        private void ChangeDb(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.MySql:
                    //InitMysqlDatabase();
                    InitMysqlService();
                    break;
                case DbType.Pgsql:
                    //InitPgsqlDatabase();
                    InitPgsqlService();
                    break;
            }
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
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

            //var count = orderHeaderRepository
            //    .LeftJoin2(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
            //    .Count(x => x.T1.Id == orderHeader.Id);
            //Assert.Equal(2, count);
        }

        [Fact, Priority(120)]
        public async Task TestJoinGroupAsync()
        {
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var addressRepository = serviceProvider.GetService<IAddressRepository>();

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
            var c2 = await orderHeaderRepository
                .LeftJoin2(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
                .GroupBy(x => x.T1.Id)
                .Select(x => new { x.Key, Count2 = x.Max(y => y.T1.CustomerId) })
                .ToListAsync();

        }
    }
}
