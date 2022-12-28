using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
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
        /// 测试插入实体和更新实体前的自定义函数
        /// </summary>
        [Fact, Priority(113)]
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
            sb.AppendLine("CREATE TABLE \"NullableTable2\" (");
            sb.AppendLine("    \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,");
            sb.AppendLine("    \"Int2\" INTEGER NULL,");
            sb.AppendLine("    \"Long2\" INTEGER NULL,");
            sb.AppendLine("    \"Float2\" REAL NULL,");
            sb.AppendLine("    \"Double2\" REAL NULL,");
            sb.AppendLine("    \"Decimal2\" TEXT NULL,");
            sb.AppendLine("    \"Decimal3\" TEXT NULL,");
            sb.AppendLine("    \"Guid2\" TEXT NULL,");
            sb.AppendLine("    \"Short2\" INTEGER NULL,");
            sb.AppendLine("    \"DateTime2\" TEXT NULL,");
            sb.AppendLine("    \"Bool2\" INTEGER NULL,");
            sb.AppendLine("    \"TimeSpan2\" TEXT NULL,");
            sb.AppendLine("    \"Byte2\" INTEGER NULL,");
            sb.AppendLine("    \"String2\" TEXT NULL,");
            sb.AppendLine("    \"String3\" TEXT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(0, result[0].Descriptions.Count);

            //dbGenerator.ExecuteGenerateSql(result[0]);
            sb.Clear();
            sb.AppendLine("CREATE TABLE \"NotNullableTable2\" (");
            sb.AppendLine("    \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,");
            sb.AppendLine("    \"Int2\" INTEGER NOT NULL,");
            sb.AppendLine("    \"Long2\" INTEGER NOT NULL,");
            sb.AppendLine("    \"Float2\" REAL NOT NULL,");
            sb.AppendLine("    \"Double2\" REAL NOT NULL,");
            sb.AppendLine("    \"Decimal2\" TEXT NOT NULL,");
            sb.AppendLine("    \"Decimal3\" TEXT NOT NULL,");
            sb.AppendLine("    \"Guid2\" TEXT NOT NULL,");
            sb.AppendLine("    \"Short2\" INTEGER NOT NULL,");
            sb.AppendLine("    \"DateTime2\" TEXT NOT NULL,");
            sb.AppendLine("    \"Bool2\" INTEGER NOT NULL,");
            sb.AppendLine("    \"TimeSpan2\" TEXT NOT NULL,");
            sb.AppendLine("    \"Byte2\" INTEGER NOT NULL,");
            sb.AppendLine("    \"String2\" TEXT NOT NULL,");
            sb.AppendLine("    \"String3\" TEXT NOT NULL");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1].Body);

            sb.Clear();
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
            Assert.Equal(1, result.Count());
            Assert.Equal(0, result[0].Descriptions.Count);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE NullableTable ADD \"int3\" INTEGER NULL", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE \"SpecifiedMapTestTable\" (");
            sb.AppendLine("    \"NormalTxt\" TEXT NULL,");
            sb.AppendLine("    \"SpecifiedTxt\" real NULL");
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
