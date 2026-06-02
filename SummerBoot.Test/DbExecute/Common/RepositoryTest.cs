using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Mysql;
using SummerBoot.Oracle;
using SummerBoot.Pgsql;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using SummerBoot.Repository.TypeHandler.Dialect.Mysql;
using SummerBoot.Sqlite;
using SummerBoot.SqlServer;
using SummerBoot.Test.Common;
using SummerBoot.Test.Common.Dto;
using SummerBoot.Test.DbExecute.Common.Db;
using SummerBoot.Test.DbExecute.Common.Models;
using SummerBoot.Test.DbExecute.Common.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Test.DbExecute.Common
{
    public partial class RepositoryTest : IClassFixture<DatabaseInitFixture>
    {
        private readonly DatabaseInitFixture databaseInitFixture;
        private readonly ITestOutputHelper output;

        public RepositoryTest(DatabaseInitFixture databaseInitFixture, ITestOutputHelper output)
        {
            this.databaseInitFixture = databaseInitFixture;
            this.output = output;
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
                it.AddMysql<IUnitOfWork1>(connectionString,
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
                        x.GuidToString = true;
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
                it.AddOracle<IUnitOfWork1>(connectionString,
                    x =>
                    {
                        x.DebugSqlAction = debugSqlAction;
                        x.TableNameMapping = a => a.ToUpper();
                        x.ColumnNameMapping = a => a.ToUpper();
                        x.BindRepositoriesWithAttribute<AutoRepositoryAttribute>();
                        x.BindManualRepositoriesWithAttribute<ManualRepositoryAttribute>();
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
                it.AddPgsql<IUnitOfWork1>(connectionString,
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
                it.AddSqlServer<IUnitOfWork1>(connectionString,
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
                it.AddSqlite<IUnitOfWork1>(databaseString,
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
                    var conn = $"Data Source=./{GetRandomName()}.db";
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
                OrderNo = GetRandomName(),
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var dbOrder = await orderHeaderRepository.FirstOrDefaultAsync(x => x.OrderNo == orderHeader.OrderNo);
            Assert.Equal(dbOrder.Id, orderHeader.Id);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestGetAllAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = GetRandomName(),
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var dbOrders = await orderHeaderRepository.GetAllAsync();
            Assert.True(dbOrders.Count > 0);
            Assert.True(dbOrders.Any(x => x.Id == orderHeader.Id));
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestSumAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).SumAsync(x => x.State);
            Assert.Equal(101, r1);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestMaxAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).MaxAsync(x => x.State);
            Assert.Equal(100, r1);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestMinAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).MinAsync(x => x.State);
            Assert.Equal(1, r1);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestAverageAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).AverageAsync(x => x.State);
            Assert.Equal(50, r1);
        }


        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestGetAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = GetRandomName(),
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var dbOrder = await orderHeaderRepository.GetAsync(orderHeader.Id);
            Assert.Equal(dbOrder.OrderNo, orderHeader.OrderNo);
            Assert.Equal(dbOrder.State, orderHeader.State);
            Assert.True(TestUtils.CompareTwoDate(dbOrder.CreateTime, orderHeader.CreateTime));
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestStringLength(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var name = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = name,
                State = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var dbOrder2 = await orderHeaderRepository.FirstOrDefaultAsync(x => x.Id == orderHeader.Id && x.OrderNo.Length > 0);

            var dbOrder = await orderHeaderRepository.FirstOrDefaultAsync(x => x.Id == orderHeader.Id && x.OrderNo.Length == name.Length);
            TestUtils.CompareTwoModel(dbOrder2, orderHeader);
            TestUtils.CompareTwoModel(dbOrder, orderHeader);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestOrderByFirstOrDefaultAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 2,
                CustomerId = 2
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var dbOrder = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).OrderBy(x => x.State)
                .FirstOrDefaultAsync();
            Assert.Equal(dbOrder.CustomerId, orderHeader.CustomerId);
            Assert.Equal(dbOrder.State, orderHeader.State);
            Assert.True(TestUtils.CompareTwoDate(dbOrder.CreateTime, orderHeader.CreateTime));

        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestUpdateAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var guid = Guid.NewGuid();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0, DateTimeKind.Local);
            var a = new NullableTable()
            {
                Int2 = 1,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = dateNow.AddMinutes(1),
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = guid,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2
            };

            await nullableTableRepository.InsertAsync(a);
            var dbModel = await nullableTableRepository.FirstOrDefaultAsync(x => x.Guid2 == guid);
            Assert.NotNull(dbModel);
            var updateGuid = Guid.NewGuid();
            dbModel.Int2 = 2;
            dbModel.Bool2 = false;
            dbModel.Byte2 = 2;
            dbModel.DateTime2 = dateNow.AddMinutes(3);
            dbModel.Decimal2 = 2m;
            dbModel.Decimal3 = 2.2m;
            dbModel.Double2 = 2.2;
            dbModel.Float2 = (float)2.2;
            dbModel.Guid2 = updateGuid;
            dbModel.Short2 = 2;
            dbModel.TimeSpan2 = TimeSpan.FromHours(2);
            dbModel.String2 = "sb2";
            dbModel.String3 = "sb2";
            dbModel.Long2 = 2;
            await nullableTableRepository.UpdateAsync(dbModel);
            var dbModel2 = await nullableTableRepository.GetAsync(dbModel.Id);
            Assert.NotNull(dbModel2);
            Assert.Equal(dbModel2.Int2, 2);
            Assert.Equal(dbModel2.Bool2, false);
            Assert.Equal(dbModel2.Byte2, (byte)2);
            Assert.Equal(dbModel2.DateTime2, dateNow.AddMinutes(3));
            Assert.Equal(dbModel2.Decimal2, 2m);
            Assert.Equal(dbModel2.Decimal3, 2.2m);
            Assert.Equal(dbModel2.Double2, 2.2);
            Assert.Equal(dbModel2.Float2, (float)2.2);
            Assert.Equal(dbModel2.Guid2, updateGuid);
            Assert.Equal(dbModel2.Short2, (short)2);
            Assert.Equal(dbModel2.TimeSpan2, TimeSpan.FromHours(2));
            Assert.Equal(dbModel2.String2, "sb2");
            Assert.Equal(dbModel2.String3, "sb2");
            Assert.Equal(dbModel2.Long2, 2);
            Assert.Equal(dbModel2.Id, dbModel.Id);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestDeleteAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var guid = Guid.NewGuid();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0);
            var a = new NullableTable()
            {
                Int2 = 1,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = dateNow.AddMinutes(1),
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = guid,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2
            };

            await nullableTableRepository.InsertAsync(a);
            var dbModel = await nullableTableRepository.GetAsync(a.Id);
            Assert.NotNull(dbModel);
            await nullableTableRepository.DeleteAsync(a);
            var dbModel2 = await nullableTableRepository.GetAsync(a.Id);
            Assert.Null(dbModel2);
        }

        /// <summary>
        /// 测试带事务的快速批量插入
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestFastBatchInsertWithDbTransactionAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 20000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();

            var nullableTableList = new List<NullableTable>();
            var name = GetRandomName();
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
                    String2 = name,
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

            try
            {
                unitOfWork.BeginTransaction();
                await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
                throw new Exception("test");
                unitOfWork.Commit();
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }
            var count = await nullableTableRepository.CountAsync(x => x.String2 == name);
            Assert.Equal(0, count);

            try
            {
                var sw = Stopwatch.StartNew();
                unitOfWork.BeginTransaction();
                await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
                unitOfWork.Commit();
                sw.Stop();
                output.WriteLine(dbType.ToString() + ":" + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }

            var count2 = await nullableTableRepository.CountAsync(x => x.String2 == name);
            Assert.Equal(total, count2);
            var dbModel = await nullableTableRepository.FirstOrDefaultAsync(x => x.String2 == name && x.Guid2 == guid);
            TestUtils.CompareTwoModel(dbModel, nullableTableList[0], new List<string>() { "Id" });
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestFirstOrDefaultByExpressionAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var guid = Guid.NewGuid();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0);
            var dbModel = new NullableTable()
            {
                Int2 = 1,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = dateNow.AddMinutes(1),
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = guid,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2
            };

            await nullableTableRepository.InsertAsync(dbModel);
            var dbModel12 = await nullableTableRepository.FirstOrDefaultAsync(x => x.Id == dbModel.Id
                && x.Int2 == dbModel.Int2
                && x.Bool2 == dbModel.Bool2
                && x.Byte2 == dbModel.Byte2
                && x.DateTime2 == dbModel.DateTime2
                && x.Decimal2 == dbModel.Decimal2
                && x.Decimal3 == dbModel.Decimal3
                && x.Float2 == dbModel.Float2
                && x.Guid2 == dbModel.Guid2
                && x.Short2 == dbModel.Short2
                && x.TimeSpan2 == dbModel.TimeSpan2
                && x.String2 == dbModel.String2
                && x.String3 == dbModel.String3
                && x.Long2 == dbModel.Long2
                && x.Double2 == dbModel.Double2);
            Assert.NotNull(dbModel12);
        }


        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestDeleteByExpressionAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var guid = Guid.NewGuid();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0);
            var a = new NullableTable()
            {
                Int2 = 1,
                Bool2 = true,
                Byte2 = 1,
                DateTime2 = dateNow.AddMinutes(1),
                Decimal2 = 1m,
                Decimal3 = 1.1m,
                Double2 = 1.1,
                Float2 = (float)1.1,
                Guid2 = guid,
                Short2 = 1,
                TimeSpan2 = TimeSpan.FromHours(1),
                String2 = "sb",
                String3 = "sb",
                Long2 = 2
            };

            await nullableTableRepository.InsertAsync(a);
            var dbModel = await nullableTableRepository.GetAsync(a.Id);

            Assert.NotNull(dbModel);

            var affectedRow = await nullableTableRepository.DeleteAsync(x => x.Id == dbModel.Id && x.Double2 == dbModel.Double2 && x.Float2 == dbModel.Float2);
            Assert.Equal(1, affectedRow);
            var dbModel2 = await nullableTableRepository.GetAsync(a.Id);
            Assert.Null(dbModel2);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestPageAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };

            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 2,
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var pageResult = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).ToPageAsync(new Pageable(1, 10));
            Assert.Equal(2, pageResult.TotalPages);
            Assert.Equal(orderNo, pageResult.Data[0].OrderNo);
            Assert.Equal(orderNo, pageResult.Data[1].OrderNo);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestWhereToListAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };

            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 2,
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).ToListAsync();
            Assert.Equal(2, r1.Count);
            Assert.Equal(orderNo, r1[0].OrderNo);
            Assert.Equal(orderNo, r1[1].OrderNo);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestPage2Async(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };

            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 2,
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var pageResult = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).Skip(1).Take(1).ToPageAsync();
            Assert.Equal(2, pageResult.TotalPages);
            Assert.Equal(orderNo, pageResult.Data[0].OrderNo);
        }


        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestPageOrderByAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };

            await orderHeaderRepository.InsertAsync(orderHeader);
            var orderHeader2 = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 2,
            };
            await orderHeaderRepository.InsertAsync(orderHeader2);
            var pageResult = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).OrderBy(x => x.CustomerId).ToPageAsync(new Pageable(1, 10));
            Assert.Equal(2, pageResult.TotalPages);
            Assert.Equal(1, pageResult.Data[0].CustomerId);
            Assert.Equal(2, pageResult.Data[1].CustomerId);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestCountAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).CountAsync();
            Assert.Equal(1, r1);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestOrderByCountAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 1,
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var r1 = await orderHeaderRepository.Where(x => x.OrderNo == orderNo).OrderBy(x => x.OrderNo).CountAsync();
            Assert.Equal(1, r1);
        }



        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestWhereConditionContainOtherMethod(DbType dbType)
        {

            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var nullableTableList = new List<NullableTable>();
            var dateNow = new DateTime(2023, 10, 24, 17, 0, 0);
            var name = GetRandomName();
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
                    String2 = name,
                    String3 = "sb",
                    Long2 = 2
                };

                nullableTableList.Add(a);
            }

            await nullableTableRepository.InsertAsync(nullableTableList);

            var result = await nullableTableRepository.Where(it => it.String2 == TestWhereConditionContainStringMethodItem(name) && it.DateTime2 >= dateNow.AddMinutes(3)).ToListAsync();
            Assert.Equal(2, result.Count);

            var result2 = await nullableTableRepository.Where(it => it.String2 == TestWhereConditionContainStringMethodItem(name) && it.DateTime2 >= TestWhereConditionContainDateTimeMethodItem(dateNow)).ToListAsync();
            Assert.Equal(2, result2.Count);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestWhereConditionHaveNullableValue(DbType dbType)
        {

            ChangeDb(dbType);
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var nullableTableList = new List<NullableTable>();
            var name = GetRandomName();
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
                    String2 = name,
                    String3 = "sb",
                    Long2 = 2,
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };

                nullableTableList.Add(a);
            }

            await nullableTableRepository.InsertAsync(nullableTableList);
            var list = new List<int>() { 1, 11, 111 };
            var result = await nullableTableRepository.Where(it => it.String2 == TestWhereConditionContainStringMethodItem(name) && list.Contains(it.Int2.Value)).ToListAsync();
            Assert.Equal(1, result.Count);
            Assert.Equal(1, result[0].Int2);
            var result2 = await nullableTableRepository.Where(it => it.String2 == TestWhereConditionContainStringMethodItem(name) && it.Int2.Value == new { value = 2 }.value).ToListAsync();
            Assert.Equal(1, result2.Count);
            Assert.Equal(2, result2[0].Int2);
        }

        /// <summary>
        /// 测试where条件中右边的值为dto的属性，并且同时测试从list里获取索引为0或者1的值
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestParameterWithListPropertyDtoAndGetItem(DbType dbType)
        {

            ChangeDb(dbType);
            var addressRepository = serviceProvider.GetService<IAddressRepository>();

            var baseTime = new DateTime(2023, 10, 24, 17, 0, 0);
            var date1 = baseTime.AddMinutes(-10);
            var date2 = baseTime.AddMinutes(-5);
            var date3 = baseTime.AddMinutes(-7);
            var date4 = baseTime.AddMinutes(-12);
            var city = GetRandomName();
            var address1 = new Address()
            {
                CustomerId = 1,
                City = city,
                CreateOn = date3
            };
            var address2 = new Address()
            {
                CustomerId = 2,
                City = city,
                CreateOn = date4
            };

            await addressRepository.InsertAsync(address1);
            await addressRepository.InsertAsync(address2);

            var p = new ParameterWithListPropertyDto()
            {
                DateTimes = new List<DateTime>() { date1, date2 }
            };

            var addressList = await addressRepository.Where(it => it.City == city && it.CreateOn > p.DateTimes[0] && it.CreateOn < p.DateTimes[1]).ToListAsync();

            Assert.Equal(1, addressList.Count);
            Assert.True(TestUtils.CompareTwoDate(date3, addressList[0].CreateOn));
            Assert.Equal(city, addressList[0].City);
        }

        /// <summary>
        ///测试插入实体和更新实体前的自定义函数
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestBeforeInsertAndUpdateEvent(DbType dbType)
        {

            ChangeDb(dbType);
            var guidModelRepository = serviceProvider.GetService<IGuidModelRepository>();
            var dbGenerator1 = serviceProvider.GetService<IDbGenerator1>();
            //dbGenerator1.GenerateSql(new List<Type>() { typeof(GuidModel) });
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
            await guidModelRepository.InsertAsync(guidModel2);
            Assert.Equal("abc", guidModel2.Address);
            guidModel2.Name = "ccd";
            await guidModelRepository.UpdateAsync(guidModel2);
            Assert.Equal("ppp", guidModel2.Address);
        }


        /// <summary>
        ///测试id类型为guid的model的增删改查
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestModelUseGuidAsId(DbType dbType)
        {
            ChangeDb(dbType);
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
            var dbGuidModel2 = await guidModelRepository.FirstOrDefaultAsync(it => it.Id == id);
            Assert.Equal(guidModel, dbGuidModel2);
            dbGuidModel2.Name = "sb2";
            await guidModelRepository.UpdateAsync(dbGuidModel2);
            var dbGuidModel3 = await guidModelRepository.Where(it => it.Name == "sb2").ToListAsync();
            Assert.Equal(id, dbGuidModel3.FirstOrDefault()?.Id);
            await guidModelRepository.DeleteAsync(dbGuidModel3.FirstOrDefault());
            var nullDbGuidModel = await guidModelRepository.GetAsync(id);
            Assert.Null(nullDbGuidModel);
        }


        /// <summary>
        ///测试根据实体类创建数据库表和进行插入查询对照
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestCreateTableFromEntityAndCrud(DbType dbType)
        {
            ChangeDb(dbType);
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var nullableTable2Repository = serviceProvider.GetService<INullableTable2Repository>();
            var sqls = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2) });
            foreach (var sql in sqls)
            {
                dbGenerator.ExecuteGenerateSql(sql);
            }

            var now = DateTime.Now;
            var name = GetRandomName();
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
                String2 = name,
                String3 = "sb",
                TimeSpan2 = TimeSpan.FromHours(1)
            };
            await nullableTable2Repository.InsertAsync(entity);

            var dbEntity = await nullableTable2Repository.FirstOrDefaultAsync(it => it.String2 == name);

            CompareTwoNullable(entity, dbEntity);
            var name2 = GetRandomName();
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
                String2 = name2,
                String3 = null,
                TimeSpan2 = null
            };
            await nullableTable2Repository.InsertAsync(entity2);

            var dbEntity2 = await nullableTable2Repository.FirstOrDefaultAsync(it => it.String2 == name2);

            CompareTwoNullable(entity2, dbEntity2);
        }



        /// <summary>
        ///测试表名字段名映射
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestTableColumnMap(DbType dbType)
        {
            ChangeDb(dbType);
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var tableColumnMapRepository = serviceProvider.GetService<ITableColumnMapRepository>();
            var name = GetRandomName();
            await customerRepository.InsertAsync(new Customer() { Name = name });
            var customer = await tableColumnMapRepository.FirstOrDefaultAsync(it => it.CustomerName == name);
            Assert.NotNull(customer);
            Assert.Equal(name, customer.CustomerName);
        }

        /// <summary>
        ///测试根据数据库表生成c#类
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestGenerateCsharpClassByDatabaseInfo(DbType dbType)
        {
            ChangeDb(dbType);
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();

            var result = dbGenerator.GenerateCsharpClass(new List<string>() { "Customer", "NullableTable", "NotNullableTable" }, "abc");
            Assert.Equal(3, result.Count);

            if (dbType == DbType.SqlServer)
            {
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
            else if ((dbType == DbType.MySql))
            {
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
                sb.AppendLine("      public string Guid2 { get; set; }");
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
                sb.AppendLine("      public string Guid2 { get; set; }");
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
            else if (dbType == DbType.Oracle)
            {
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
            else if (dbType == DbType.Sqlite)
            {
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
                sb.AppendLine("      [Column(\"BirthDay\")]");
                sb.AppendLine("      public string BirthDay { get; set; }");
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
                sb.AppendLine("      [Column(\"Enum2\")]");
                sb.AppendLine("      public long? Enum2 { get; set; }");
                sb.AppendLine("      [Column(\"TestInt3\")]");
                sb.AppendLine("      public long? TestInt3 { get; set; }");
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
            else if (dbType == DbType.Pgsql)
            {
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
                sb.AppendLine("      public short? Byte2 { get; set; }");
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
                sb.AppendLine("      public short Byte2 { get; set; }");
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
        }

        /// <summary>
        ///测试根据c#类生成数据库表
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestGenerateDatabaseTableByCsharpClass(DbType dbType)
        {
            ChangeDb(dbType);
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) }, isForce: true);
            Assert.Equal(2, result.Count());

            if (dbType == DbType.MySql)
            {
                var sb = new StringBuilder();
                sb.AppendLine("CREATE TABLE test.`NullableTable2` (");
                sb.AppendLine("    `Id` int NOT NULL AUTO_INCREMENT,");
                sb.AppendLine("    `Int2` int NULL,");
                sb.AppendLine("    `Long2` bigint NULL,");
                sb.AppendLine("    `Float2` float NULL,");
                sb.AppendLine("    `Double2` double NULL,");
                sb.AppendLine("    `Decimal2` decimal(18,2) NULL,");
                sb.AppendLine("    `Decimal3` decimal(20,4) NULL,");
                sb.AppendLine("    `Guid2` char(36) NULL,");
                sb.AppendLine("    `Short2` smallint NULL,");
                sb.AppendLine("    `DateTime2` datetime NULL,");
                sb.AppendLine("    `Bool2` tinyint(1) NULL,");
                sb.AppendLine("    `TimeSpan2` time NULL,");
                sb.AppendLine("    `Byte2` tinyint unsigned NULL,");
                sb.AppendLine("    `String2` varchar(100) NULL,");
                sb.AppendLine("    `String3` text NULL,");
                sb.AppendLine("    `Enum2` int NULL,");
                sb.AppendLine("    `TestInt3` int NULL,");
                sb.AppendLine("    PRIMARY KEY (`Id`)");
                sb.AppendLine(")");
                var exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);

                Assert.Equal(4, result[0].Descriptions.Count);
                Assert.Equal("ALTER TABLE test.`NullableTable2` COMMENT = 'NullableTable2'", result[0].Descriptions[0]);
                Assert.Equal("ALTER TABLE test.`NullableTable2` MODIFY `Int2` int NULL COMMENT 'Int2'", result[0].Descriptions[1]);
                Assert.Equal("ALTER TABLE test.`NullableTable2` MODIFY `Long2` bigint NULL COMMENT 'Long2'", result[0].Descriptions[2]);
                Assert.Equal("ALTER TABLE test.`NullableTable2` MODIFY `TestInt3` int NULL COMMENT 'Int2'", result[0].Descriptions[3]);

                sb.Clear();
                sb.AppendLine("CREATE TABLE test.`NotNullableTable2` (");
                sb.AppendLine("    `Id` int NOT NULL AUTO_INCREMENT,");
                sb.AppendLine("    `Int2` int NOT NULL,");
                sb.AppendLine("    `Long2` bigint NOT NULL,");
                sb.AppendLine("    `Float2` float NOT NULL,");
                sb.AppendLine("    `Double2` double NOT NULL,");
                sb.AppendLine("    `Decimal2` decimal(18,2) NOT NULL,");
                sb.AppendLine("    `Decimal3` decimal(20,4) NOT NULL,");
                sb.AppendLine("    `Guid2` char(36) NOT NULL,");
                sb.AppendLine("    `Short2` smallint NOT NULL,");
                sb.AppendLine("    `DateTime2` datetime NOT NULL,");
                sb.AppendLine("    `Bool2` tinyint(1) NOT NULL,");
                sb.AppendLine("    `TimeSpan2` time NOT NULL,");
                sb.AppendLine("    `Byte2` tinyint unsigned NOT NULL,");
                sb.AppendLine("    `String2` varchar(100) NOT NULL,");
                sb.AppendLine("    `String3` text NOT NULL,");
                sb.AppendLine("    PRIMARY KEY (`Id`)");
                sb.AppendLine(")");

                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[1].Body);

                sb.Clear();
                result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
                Assert.Equal(1, result.Count());
                Assert.Equal(1, result[0].Descriptions.Count);
                Assert.Equal("ALTER TABLE test.`NullableTable` MODIFY `int3` int NULL COMMENT 'test add column'", result[0].Descriptions[0]);
                Assert.Equal(1, result[0].FieldModifySqls.Count);
                Assert.Equal("ALTER TABLE test.`NullableTable` ADD `int3` int NULL", result[0].FieldModifySqls[0]);

                result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
                Assert.Equal(1, result.Count());
                sb.Clear();
                sb.AppendLine("CREATE TABLE test.`SpecifiedMapTestTable` (");
                sb.AppendLine("    `NormalTxt` text NULL,");
                sb.AppendLine("    `SpecifiedTxt` text NULL");
                sb.AppendLine(")");
                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);

            }
            else if (dbType == DbType.SqlServer)
            {
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
            }
            else if (dbType == DbType.Oracle)
            {
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
                sb.AppendLine("    \"SPECIFIEDTXT\" text");
                sb.AppendLine(")");
                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);
            }
            else if (dbType == DbType.Sqlite)
            {
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
                sb.AppendLine("    `String3` TEXT NULL,");
                sb.AppendLine("    `Enum2` INTEGER NULL,");
                sb.AppendLine("    `TestInt3` INTEGER NULL");
                sb.AppendLine(")");
                var exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);

                Assert.Equal(0, result[0].Descriptions.Count);

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
                sb.AppendLine("    `SpecifiedTxt` text NULL");
                sb.AppendLine(")");
                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);
            }
            else if (dbType == DbType.Pgsql)
            {
                var sb = new StringBuilder();
                sb.AppendLine("CREATE TABLE public.\"NullableTable2\" (");
                sb.AppendLine("    \"Id\" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY,");
                sb.AppendLine("    \"Int2\" int4 NULL,");
                sb.AppendLine("    \"Long2\" int8 NULL,");
                sb.AppendLine("    \"Float2\" float4 NULL,");
                sb.AppendLine("    \"Double2\" float8 NULL,");
                sb.AppendLine("    \"Decimal2\" numeric(18,2) NULL,");
                sb.AppendLine("    \"Decimal3\" numeric(20,4) NULL,");
                sb.AppendLine("    \"Guid2\" uuid NULL,");
                sb.AppendLine("    \"Short2\" int2 NULL,");
                sb.AppendLine("    \"DateTime2\" timestamp NULL,");
                sb.AppendLine("    \"Bool2\" bool NULL,");
                sb.AppendLine("    \"TimeSpan2\" interval NULL,");
                sb.AppendLine("    \"Byte2\" int2 NULL,");
                sb.AppendLine("    \"String2\" varchar(100) NULL,");
                sb.AppendLine("    \"String3\" text NULL,");
                sb.AppendLine("    \"Enum2\" int4 NULL,");
                sb.AppendLine("    \"TestInt3\" int4 NULL,");
                sb.AppendLine(" CONSTRAINT NullableTable2_pk PRIMARY KEY (\"Id\")");
                sb.AppendLine(")");
                var exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);

                Assert.Equal(4, result[0].Descriptions.Count);
                Assert.Equal("COMMENT ON TABLE public.\"NullableTable2\" IS 'NullableTable2'", result[0].Descriptions[0]);
                Assert.Equal("COMMENT ON COLUMN public.\"NullableTable2\".\"Int2\" IS 'Int2'", result[0].Descriptions[1]);
                Assert.Equal("COMMENT ON COLUMN public.\"NullableTable2\".\"Long2\" IS 'Long2'", result[0].Descriptions[2]);
                Assert.Equal("COMMENT ON COLUMN public.\"NullableTable2\".\"TestInt3\" IS 'Int2'", result[0].Descriptions[3]);

                sb.Clear();
                sb.AppendLine("CREATE TABLE public.\"NotNullableTable2\" (");
                sb.AppendLine("    \"Id\" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY,");
                sb.AppendLine("    \"Int2\" int4 NOT NULL,");
                sb.AppendLine("    \"Long2\" int8 NOT NULL,");
                sb.AppendLine("    \"Float2\" float4 NOT NULL,");
                sb.AppendLine("    \"Double2\" float8 NOT NULL,");
                sb.AppendLine("    \"Decimal2\" numeric(18,2) NOT NULL,");
                sb.AppendLine("    \"Decimal3\" numeric(20,4) NOT NULL,");
                sb.AppendLine("    \"Guid2\" uuid NOT NULL,");
                sb.AppendLine("    \"Short2\" int2 NOT NULL,");
                sb.AppendLine("    \"DateTime2\" timestamp NOT NULL,");
                sb.AppendLine("    \"Bool2\" bool NOT NULL,");
                sb.AppendLine("    \"TimeSpan2\" interval NOT NULL,");
                sb.AppendLine("    \"Byte2\" int2 NOT NULL,");
                sb.AppendLine("    \"String2\" varchar(100) NOT NULL,");
                sb.AppendLine("    \"String3\" text NOT NULL,");
                sb.AppendLine(" CONSTRAINT NotNullableTable2_pk PRIMARY KEY (\"Id\")");
                sb.AppendLine(")");

                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[1].Body);

                sb.Clear();
                result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
                Assert.Equal(1, result.Count());
                Assert.Equal(1, result[0].Descriptions.Count);
                Assert.Equal("COMMENT ON COLUMN public.\"NullableTable\".\"int3\" IS 'test add column'", result[0].Descriptions[0]);
                Assert.Equal(1, result[0].FieldModifySqls.Count);
                Assert.Equal("ALTER TABLE public.\"NullableTable\" ADD \"int3\" int4 NULL", result[0].FieldModifySqls[0]);

                result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
                Assert.Equal(1, result.Count());
                sb.Clear();
                sb.AppendLine("CREATE TABLE public.\"SpecifiedMapTestTable\" (");
                sb.AppendLine("    \"NormalTxt\" text NULL,");
                sb.AppendLine("    \"SpecifiedTxt\" text NULL");
                sb.AppendLine(")");
                exceptStr = sb.ToString();
                Assert.Equal(exceptStr
                    , result[0].Body);
            }
        }


        /// <summary>
        ///测试事务
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestTransaction(DbType dbType)
        {
            ChangeDb(dbType);
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
            var name = GetRandomName();
            try
            {
                unitOfWork.BeginTransaction();
                await customerRepository.InsertAsync(new Customer() { Name = name });
                await customerRepository.InsertAsync(new Customer() { Name = name });
                unitOfWork.Commit();
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }

            var customers = await customerRepository.Where(it => it.Name == name).ToListAsync();
            Assert.Equal(2, customers.Count);
            Assert.Equal(name, customers[0].Name);
        }

        /// <summary>
        ///测试事务回滚
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestTransactionRollBack(DbType dbType)
        {
            ChangeDb(dbType);
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
            var name = GetRandomName();
            try
            {
                unitOfWork.BeginTransaction();
                await customerRepository.InsertAsync(new Customer() { Name = name });
                throw new Exception("test");
                await customerRepository.InsertAsync(new Customer() { Name = name });
                unitOfWork.Commit();
            }
            catch (Exception e)
            {
                unitOfWork.RollBack();
            }

            var customers = await customerRepository.Where(it => it.Name == name).ToListAsync();
            Assert.Equal(0, customers.Count);
        }

        /// <summary>
        ///测试事务回滚
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestFastBatchInsert(DbType dbType)
        {
            ChangeDb(dbType);
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var name = GetRandomName();
            var total = 20000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
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
                    Guid2 = guid,
                    Id = 0,
                    Short2 = 1,
                    TimeSpan2 = TimeSpan.FromHours(1),
                    String2 = name,
                    String3 = "sb",
                    Long2 = 2,
                    Enum2 = Model.Enum2.y,
                    Int3 = 4
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                    a.Int2 = 1;
                }
                nullableTableList.Add(a);
            }

            var sw = Stopwatch.StartNew();
            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
            sw.Stop();
            var s = sw.ElapsedMilliseconds;
            output.WriteLine($"cost {s}ms");
            var count = await nullableTableRepository.CountAsync(x => x.String2 == name);
            Assert.Equal(total, count);
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
        private string TestWhereConditionContainStringMethodItem(string name)
        {
            return name;
        }

        private DateTime TestWhereConditionContainDateTimeMethodItem(DateTime dateTime)
        {
            return dateTime.AddMinutes(3);
        }

        private string GetRandomName()
        {
            return Guid.NewGuid().ToString("N");
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
            var orderNo = Guid.NewGuid().ToString();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 100
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

            var r1 = await orderHeaderRepository
                .LeftJoin(orderDetailRepository, x => x.T1.Id == x.T2.OrderHeaderId)
                .Where(x => x.T1.OrderNo == orderNo)
                .GroupBy(x => x.T1.Id)
                .Select(x => new { x.Key, Count2 = x.Max(y => y.T1.CustomerId) })
                .ToListAsync();
            Assert.Equal(1, r1.Count);
            Assert.Equal(orderHeader.Id, r1.First().Key);
            Assert.Equal(orderHeader.CustomerId, r1.First().Count2);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestQueryFirstOrDefaultAsync(DbType dbType)
        {
            ChangeDb(dbType);

            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var repositoryOption = serviceProvider.GetService<RepositoryOption>();
            var databaseUnit = repositoryOption.DatabaseUnits.First().Value;
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var tableName = GetTableName(databaseUnit, nameof(OrderHeader));
            var parameterName = nameof(OrderHeader.OrderNo);
            var columnName = GetColumnName(databaseUnit, parameterName);

            var parameter = new SummerBoot.Repository.Core.DynamicParameters();
            parameter.Add(parameterName, orderNo);
            var dbModel = await orderHeaderRepository.QueryFirstOrDefaultAsync<OrderHeader>($"select * from {tableName} where {columnName}={databaseUnit.ParameterNamePrefix + parameterName}", parameter);
            TestUtils.CompareTwoModel(dbModel, orderHeader);
        }

        private string GetColumnName(DatabaseUnit databaseUnit,string columnName)
        {
            columnName = databaseUnit.LeftQualifiers + columnName + databaseUnit.RightQualifiers;
            if (databaseUnit.ColumnNameMapping != null)
            {
                columnName = databaseUnit.ColumnNameMapping(columnName);
            }

            return columnName;
        }

        private string GetTableName(DatabaseUnit databaseUnit, string tableName)
        {
            tableName = databaseUnit.LeftQualifiers + tableName + databaseUnit.RightQualifiers;
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }

            return tableName;
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestQueryListAsync(DbType dbType)
        {
            ChangeDb(dbType);

            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var repositoryOption = serviceProvider.GetService<RepositoryOption>();
            var databaseUnit = repositoryOption.DatabaseUnits.First().Value;
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader);
            var name = GetRandomName();
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

            var tableName = GetTableName(databaseUnit, nameof(OrderHeader));
            var parameterName = nameof(OrderHeader.OrderNo);
            var columnName = GetColumnName(databaseUnit, parameterName);
            var orderDetailTableName = GetTableName(databaseUnit, nameof(OrderDetail));

            var idColumnName = GetColumnName(databaseUnit, nameof(OrderHeader.Id));
            var orderHeaderIdColumnName = GetColumnName(databaseUnit, nameof(OrderDetail.OrderHeaderId));
            var parameter = new SummerBoot.Repository.Core.DynamicParameters();
            parameter.Add(parameterName, orderNo);
            var dbModels = await orderDetailRepository.QueryListAsync<OrderDetail>($"select b.* from {tableName} a left join {orderDetailTableName} b on a.{idColumnName}=b.{orderHeaderIdColumnName} where {columnName}={databaseUnit.ParameterNamePrefix + parameterName}", parameter);
            TestUtils.CompareTwoModel(dbModels.First(x=>x.ProductName=="A"), orderDetail1);
            TestUtils.CompareTwoModel(dbModels.First(x => x.ProductName == "B"), orderDetail2);
        }

        [Theory]
        [InlineData(DbType.MySql)]
        [InlineData(DbType.Pgsql)]
        [InlineData(DbType.Oracle)]
        [InlineData(DbType.SqlServer)]
        [InlineData(DbType.Sqlite)]
        public async Task TestExecuteAsync(DbType dbType)
        {
            ChangeDb(dbType);

            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();
            var repositoryOption = serviceProvider.GetService<RepositoryOption>();
            var databaseUnit = repositoryOption.DatabaseUnits.First().Value;
            var orderNo = GetRandomName();
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId = 100
            };
            await orderHeaderRepository.InsertAsync(orderHeader);

            var tableName = GetTableName(databaseUnit, nameof(OrderHeader));
            var parameterName = nameof(OrderHeader.OrderNo);
            var columnName = GetColumnName(databaseUnit, parameterName);
            var stateColumnName = GetColumnName(databaseUnit, nameof(OrderHeader.State));
            var parameter = new SummerBoot.Repository.Core.DynamicParameters();
            parameter.Add(parameterName, orderNo);
            var affectRowCount = await orderHeaderRepository.ExecuteAsync($"update {tableName} set {stateColumnName}=2 where {columnName}={databaseUnit.ParameterNamePrefix + parameterName}", parameter);
            Assert.Equal(1,affectRowCount);
            var dbModel = await orderHeaderRepository.FirstOrDefaultAsync(x => x.OrderNo == orderNo);
            Assert.Equal(2, dbModel.State);
            orderHeader.State = 2;
            TestUtils.CompareTwoModel(dbModel, orderHeader);
        }
    }
}
