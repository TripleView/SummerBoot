using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using SummerBoot.Core;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.MultiDatabaseMixed.Db;
using System;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using SummerBoot.Test.MultiDatabaseMixed.Models;
using SummerBoot.Test.MultiDatabaseMixed.Repository;
using Xunit;
using Xunit.Priority;
using SummerBoot.Repository;

namespace SummerBoot.Test.MultiDatabaseMixed
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {

        private IServiceProvider serviceProvider;
        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址



        [Fact, Priority(102)]
        public void TestMysql()
        {
            InitMysqlDatabase();
            InitSqlServerDatabase();
            InitService();
            TestRepository();

        }

        public void TestRepository()
        {
            var mysqlUow = serviceProvider.GetService<IUnitOfWork1>();
            var sqlServerUow = serviceProvider.GetService<IUnitOfWork2>();
            var sqlServerCustomerRepository = serviceProvider.GetService<ISqlServerCustomerRepository>();
            var mysqlCustomerRepository = serviceProvider.GetService<IMysqlCustomerRepository>();

            var customer = new Customer() { Name = "testCustomer" };
            sqlServerCustomerRepository.Insert(customer);
            mysqlCustomerRepository.Insert(customer);
            var sqlServerCustomer= sqlServerCustomerRepository.FirstOrDefault();
            Assert.Equal("testCustomer",sqlServerCustomer.Name);
            var mysqlCustomer = mysqlCustomerRepository.FirstOrDefault();
            Assert.Equal("testCustomer", mysqlCustomer.Name);

            try
            {
                sqlServerUow.BeginTransaction();
                var customer2 = new Customer() { Name = "testCustomer2" };
                sqlServerCustomerRepository.Insert(customer2);
                var customer3 = new Customer() { Name = "testCustomer3" };
                sqlServerCustomerRepository.Insert(customer3);
                sqlServerUow.Commit();
            }
            catch (Exception e)
            {
                sqlServerUow.RollBack();
            }

            try
            {
                mysqlUow.BeginTransaction();
                var customer2 = new Customer() { Name = "testCustomer2" };
                mysqlCustomerRepository.Insert(customer2);
                var customer3 = new Customer() { Name = "testCustomer3" };
                mysqlCustomerRepository.Insert(customer3);
                throw new Exception("test");
                mysqlUow.Commit();
            }
            catch (Exception e)
            {
                mysqlUow.RollBack();
            }

            var sqlServerCount = sqlServerCustomerRepository.Count();
            var mysqlCount = mysqlCustomerRepository.Count();
            Assert.Equal(3,sqlServerCount);
            Assert.Equal(1,mysqlCount);

        }


        private void InitMysqlDatabase()
        {
            //初始化数据库
            using (var database = new MysqlDb())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }
            
        }

        private void InitSqlServerDatabase()
        {
            //初始化数据库
            using (var database = new SqlServerDb())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }
        }


        private void InitService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(CONFIG_FILE, true, true);
            var configurationRoot = build.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configurationRoot);
            services.AddSummerBoot();
            var mysqlDbConnectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(mysqlDbConnectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var sqlServerDbConnectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(sqlServerDbConnectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                //添加第一个mysql的链接
                it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(mysqlDbConnectionString,
                    x =>
                    {
                        //绑定单个仓储
                        x.BindRepository<IMysqlCustomerRepository,Customer>();
                        //通过自定义注解批量绑定仓储
                        x.BindRepositorysWithAttribute<MysqlAutoRepositoryAttribute>();
                        
                        //绑定数据库生成接口
                        x.BindDbGeneratorType<IDbGenerator1>();
                        //绑定插入前回调
                        x.BeforeInsert += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.CreateOn = DateTime.Now;
                            }
                        };
                        //绑定更新前回调
                        x.BeforeUpdate += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.LastUpdateOn = DateTime.Now;
                            }
                        };
                    });

                //添加第二个sqlserver的链接
                it.AddDatabaseUnit<SqlConnection, IUnitOfWork2>(sqlServerDbConnectionString,
                    x =>
                    {
                        x.BindRepositorysWithAttribute<SqlServerAutoRepositoryAttribute>();
                        x.BindDbGeneratorType<IDbGenerator2>();
                    });
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        }
    }
}