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
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Test.Oracle;
using Xunit;
using Xunit.Priority;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SummerBoot.Test.Sqlite;
using SummerBoot.Test.Sqlite.Db;

namespace SummerBoot.Test.DbExecute.Common
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest:IClassFixture<DatabaseInitFixture>
    {
        private readonly DatabaseInitFixture databaseInitFixture;

        public RepositoryTest(DatabaseInitFixture databaseInitFixture)
        {
            this.databaseInitFixture = databaseInitFixture;
        }

        private IServiceProvider serviceProvider;

        

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

        private void InitOracleService(Action<string, SummerBoot.Repository.Core.DynamicParameters> debugSqlAction = null)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
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
                        x.DebugSqlAction = debugSqlAction;
                        x.TableNameMapping = a => a.ToUpper();
                        x.ColumnNameMapping = a => a.ToUpper();
                        x.BindRepositoriesWithAttribute<AutoRepositoryAttribute>();
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

        private void InitSqlServerService()
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

        private void InitSqliteDatabase(string databaseString)
        {
            //初始化数据库
            using (var database = new SqliteDb(databaseString))    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }
        }

        private void InitSqliteServices(string databaseString)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(TestConstValue.CONFIG_FILE, true, true);
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
                    InitMysqlService();
                    break;
                case DbType.Pgsql:
                    InitPgsqlService();
                    break;
                case DbType.Oracle:
                    InitOracleService();
                    break;
                case DbType.SqlServer:
                    InitSqlServerService();
                    break;
                case DbType.Sqlite:
                    var conn = $"Data Source=./{Guid.NewGuid().ToString("N")}.db";
                    InitSqliteDatabase(conn);
                    InitSqliteServices(conn);
                    break;
            }
        }
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestInsertAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = Guid.NewGuid().ToString("N"),
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

           var dbOrder=  await orderHeaderRepository.FirstOrDefaultAsync(x => x.OrderNo == orderHeader.OrderNo);
           Assert.Equal(dbOrder.Id,orderHeader.Id);
        }

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

            var count = orderHeaderRepository
                .LeftJoin2(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
                .Count(x => x.T1.Id == orderHeader.Id);
            Assert.Equal(2, count);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestJoinGroupAsync(DbType dbType)
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

            var c2 = await orderHeaderRepository
                .LeftJoin2(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
                .GroupBy(x => x.T1.Id)
                .Select(x => new { x.Key, Count2 = x.Max(y => y.T1.CustomerId) })
                .ToListAsync();
            //Assert.Equal(2, count);
        }
       
    }
}
