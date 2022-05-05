using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Test.Oracle.Db;
using SummerBoot.Test.Oracle.Models;
using SummerBoot.Test.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SummerBoot.Repository.Generator;
using Xunit;

namespace SummerBoot.Test.Oracle
{
    [TestCaseOrderer("SummerBoot.Test.PriorityOrderer", "SummerBoot.Test")]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        [Fact, Order(11)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
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
        /// 测试根据c#类生成数据库表
        /// </summary>
        [Fact, Order(12)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE \"NULLABLETABLE2\" (");
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
            sb.AppendLine("    CONSTRAINT \"PK_NULLABLETABLE2\" PRIMARY KEY (\"ID\")");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(3, result[0].Descriptions.Count);
            Assert.Equal("COMMENT ON TABLE NULLABLETABLE2 IS 'NullableTable2'", result[0].Descriptions[0]);
            Assert.Equal("COMMENT ON COLUMN NULLABLETABLE2.INT2 IS 'Int2'", result[0].Descriptions[1]);
            Assert.Equal("COMMENT ON COLUMN NULLABLETABLE2.LONG2 IS 'Long2'", result[0].Descriptions[2]);
            //dbGenerator.ExecuteGenerateSql(result[0]);

            sb.Clear();
            sb.AppendLine("CREATE TABLE \"NOTNULLABLETABLE2\" (");
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
            Assert.Equal("COMMENT ON COLUMN NULLABLETABLE.INT3 IS 'test add column'", result[0].Descriptions[0]);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE NULLABLETABLE ADD \"INT3\" NUMBER(10,0)", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE \"SPECIFIEDMAPTESTTABLE\" (");
            sb.AppendLine("    \"NORMALTXT\" NVARCHAR2(2000),");
            sb.AppendLine("    \"SPECIFIEDTXT\" CLOB");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
        }

        private void InitOracleDatabase()
        {
            //初始化数据库
            using (var database = new OracleDb())    //新增
            {
                database.Database.EnsureDeleted();
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
            }

            InitService();
        }

        [Fact,Order(3)]
        public void TestOracle()
        {
            InitOracleDatabase();
            TestRepository();
        }

        [Fact,Order(2)]
        public async Task TestOracleAsync()
        {
            InitOracleDatabase();
            await TestRepositoryAsync();
        }
       

        private void InitService()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();

            var connectionString = MyConfiguration.GetConfiguration("oracleDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("oracle connectionString must not be null");
            }

            services.AddSummerBootRepository(it =>
            {
                it.DbConnectionType = typeof(OracleConnection);
                it.ConnectionString = connectionString;
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        }

        public async Task TestRepositoryAsync()
        {
            var uow = serviceProvider.GetService<IUnitOfWork>();
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
            var uow = serviceProvider.GetService<IUnitOfWork>();
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

    }
}
