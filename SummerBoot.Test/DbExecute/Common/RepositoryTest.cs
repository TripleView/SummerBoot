using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
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
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.DbExecute.Common
{
    public class RepositoryTest : IClassFixture<DatabaseInitFixture>
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
                OrderNo = Guid.NewGuid().ToString("N"),
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
            var orderNo = Guid.NewGuid().ToString("N");
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
            var orderNo = Guid.NewGuid().ToString("N");
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
            var orderNo = Guid.NewGuid().ToString("N");
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
            var orderNo = Guid.NewGuid().ToString("N");
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
                OrderNo = Guid.NewGuid().ToString("N"),
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
        public async Task TestOrderByFirstOrDefaultAsync(DbType dbType)
        {
            ChangeDb(dbType);
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();

            var orderNo = Guid.NewGuid().ToString("N");
            var orderHeader = new OrderHeader()
            {
                CreateTime = DateTime.Now,
                OrderNo = orderNo,
                State = 1,
                CustomerId=1
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

    }
}
