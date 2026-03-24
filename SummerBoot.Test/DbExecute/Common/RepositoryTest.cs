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

        private void InitOracleDatabase()
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


        private void InitSqlserverDatabase()
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
                    //InitMysqlDatabase();
                    InitMysqlService();
                    break;
                case DbType.Pgsql:
                    //InitPgsqlDatabase();
                    InitPgsqlService();
                    break;
                case DbType.Oracle:
                    InitOracleDatabase();
                    InitOracleService();
                    break;
                case DbType.SqlServer:
                    InitSqlserverDatabase();
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
