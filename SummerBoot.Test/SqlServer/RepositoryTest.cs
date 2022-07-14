using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Db;
using SummerBoot.Test.SqlServer.Models;
using SummerBoot.Test.SqlServer.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.SqlServer
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 测试事务中批量插入
        /// </summary>
        [Fact, Priority(412)]
        public async Task TestBatchInsertWithDbtransation()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
            var sw = new Stopwatch();
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
            var count2 =(await nullableTableRepository.GetAllAsync()).Count;
            Assert.Equal(4000, count2);
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(411)]
        public async Task TestBatchInsertAsync()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IDbFactory>();
            var sw = new Stopwatch();
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

            sw.Start();
            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
            var nullableTableList2 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now2,
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
                nullableTableList2.Add(a);
            }
            sw.Restart();
            await nullableTableRepository.InsertAsync(nullableTableList2);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;
            var nullableTableList3 = new List<NullableTable>();

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
                    Id = 0,
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
                nullableTableList3.Add(a);
            }
            sw.Restart();
            using (var dbConnection = new SqlConnection(connectionString))
            {
                //&SqlBulkCopyOptions.KeepNulls
                dbConnection.Open();
                var dbtran = dbConnection.BeginTransaction();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity,
                    dbtran);

                sqlBulkCopy.BatchSize = total;
                sqlBulkCopy.DestinationTableName = "NullableTable";
                sqlBulkCopy.ColumnMappings.Add("Int2", "Int2");
                sqlBulkCopy.ColumnMappings.Add("Bool2", "Bool2");
                sqlBulkCopy.ColumnMappings.Add("Byte2", "Byte2");
                sqlBulkCopy.ColumnMappings.Add("DateTime2", "DateTime2");
                sqlBulkCopy.ColumnMappings.Add("Decimal2", "Decimal2");
                sqlBulkCopy.ColumnMappings.Add("Decimal3", "Decimal3");
                sqlBulkCopy.ColumnMappings.Add("Double2", "Double2");
                sqlBulkCopy.ColumnMappings.Add("Float2", "Float2");
                sqlBulkCopy.ColumnMappings.Add("Guid2", "Guid2");
                sqlBulkCopy.ColumnMappings.Add("Short2", "Short2");
                sqlBulkCopy.ColumnMappings.Add("TimeSpan2", "TimeSpan2");

                sqlBulkCopy.ColumnMappings.Add("String2", "String2");
                sqlBulkCopy.ColumnMappings.Add("String3", "String3");
                sqlBulkCopy.ColumnMappings.Add("Long2", "Long2");
                sqlBulkCopy.ColumnMappings.Add("Enum2", "Enum2");
                sqlBulkCopy.ColumnMappings.Add("Int3", "TestInt3");

                var table = nullableTableList3.ToDataTable();
                //sqlserver替换timespan类型为long类型
                //SbUtil.ReplaceDataTableTimeSpanColumnForSqlserver(table);
                //table.Columns.Remove("id");
                await sqlBulkCopy.WriteToServerAsync(table);
                dbtran.Commit();
            }
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
            var models = nullableTableRepository.Where(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id))
                .ToList();
            Assert.Equal(3, models.Count);
            Assert.True(models[0].Equals(models[1]));
            Assert.True(models[0].Equals(models[2]));
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(410)]
        public async Task TestBatchInsert()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IDbFactory>();
            var sw = new Stopwatch();
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

            sw.Start();
            nullableTableRepository.FastBatchInsert(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
            var nullableTableList2 = new List<NullableTable>();

            for (int i = 0; i < total; i++)
            {
                var a = new NullableTable()
                {
                    Int2 = 2,
                    Bool2 = true,
                    Byte2 = 1,
                    DateTime2 = now2,
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
                nullableTableList2.Add(a);
            }
            sw.Restart();
            nullableTableRepository.Insert(nullableTableList2);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;
            var nullableTableList3 = new List<NullableTable>();

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
                    Id = 0,
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
                nullableTableList3.Add(a);
            }
            sw.Restart();
            using (var dbConnection = new SqlConnection(connectionString))
            {
                //&SqlBulkCopyOptions.KeepNulls
                dbConnection.Open();
                var dbtran = dbConnection.BeginTransaction();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity,
                    dbtran);
                sqlBulkCopy.BatchSize = total;
                sqlBulkCopy.DestinationTableName = "NullableTable";
                sqlBulkCopy.ColumnMappings.Add("Int2", "Int2");
                sqlBulkCopy.ColumnMappings.Add("Bool2", "Bool2");
                sqlBulkCopy.ColumnMappings.Add("Byte2", "Byte2");
                sqlBulkCopy.ColumnMappings.Add("DateTime2", "DateTime2");
                sqlBulkCopy.ColumnMappings.Add("Decimal2", "Decimal2");
                sqlBulkCopy.ColumnMappings.Add("Decimal3", "Decimal3");
                sqlBulkCopy.ColumnMappings.Add("Double2", "Double2");
                sqlBulkCopy.ColumnMappings.Add("Float2", "Float2");
                sqlBulkCopy.ColumnMappings.Add("Guid2", "Guid2");
                sqlBulkCopy.ColumnMappings.Add("Short2", "Short2");
                sqlBulkCopy.ColumnMappings.Add("TimeSpan2", "TimeSpan2");

                sqlBulkCopy.ColumnMappings.Add("String2", "String2");
                sqlBulkCopy.ColumnMappings.Add("String3", "String3");
                sqlBulkCopy.ColumnMappings.Add("Long2", "Long2");
                sqlBulkCopy.ColumnMappings.Add("Enum2", "Enum2");
                sqlBulkCopy.ColumnMappings.Add("Int3", "TestInt3");

                var table = nullableTableList3.ToDataTable();
                //sqlserver替换timespan类型为long类型
                //SbUtil.ReplaceDataTableTimeSpanColumnForSqlserver(table);
                //table.Columns.Remove("id");
                sqlBulkCopy.WriteToServer(table);
                dbtran.Commit();
            }
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
            var models = nullableTableRepository.Where(it => new List<int>() { 1, 2001, 4001 }.Contains(it.Id))
                .ToList();
            Assert.Equal(3, models.Count);
            Assert.True(models[0].Equals(models[1]));
            Assert.True(models[0].Equals(models[2]));
        }

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(409)]
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
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(408)]
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
        /// 测试带命名空间的情况和新增主键
        /// </summary>
        [Fact, Priority(407)]
        public void TestTableSchemaAndAddPrimaryKey()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
            var customerWithSchema2Repository = serviceProvider.GetService<ICustomerWithSchema2Repository>();
            var sb = new StringBuilder();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema) });
            sb.Clear();
            sb.AppendLine("CREATE TABLE test1.[CustomerWithSchema] (");
            sb.AppendLine("    [Name] nvarchar(max)  NULL,");
            sb.AppendLine("    [Age] int  NOT NULL,");
            sb.AppendLine("    [CustomerNo] nvarchar(max)  NULL,");
            sb.AppendLine("    [TotalConsumptionAmount] decimal(18,2)  NOT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema2) });
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [Id] int IDENTITY(1,1) PRIMARY KEY NOT NULL", result[0].FieldModifySqls[0]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [LastUpdateOn] datetime2  NULL", result[0].FieldModifySqls[1]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [LastUpdateBy] nvarchar(max)  NULL", result[0].FieldModifySqls[2]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [CreateOn] datetime2  NULL", result[0].FieldModifySqls[3]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [CreateBy] nvarchar(max)  NULL", result[0].FieldModifySqls[4]);
            Assert.Equal("ALTER TABLE test1.[CustomerWithSchema] ADD [Active] int  NULL", result[0].FieldModifySqls[5]);
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
        /// 测试根据实体类创建数据库表和进行插入查询对照
        /// </summary>
        [Fact, Priority(406)]
        public void TestCreateTableFromEntityAndCrud()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
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
                TimeSpan2 = TimeSpan.FromHours(1)
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
        [Fact, Priority(405)]
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
        [Fact, Priority(404)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
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
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      [Column(\"Name\")]");
            sb.AppendLine("      public string Name { get; set; }");
            sb.AppendLine("      [Column(\"Age\")]");
            sb.AppendLine("      public int Age { get; set; }");
            sb.AppendLine("      [Column(\"CustomerNo\")]");
            sb.AppendLine("      public string CustomerNo { get; set; }");
            sb.AppendLine("      [Column(\"TotalConsumptionAmount\")]");
            sb.AppendLine("      public decimal TotalConsumptionAmount { get; set; }");
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

        /// <summary>
        /// 测试根据c#类生成数据库表
        /// </summary>
        [Fact, Priority(403)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {

            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE dbo.[NullableTable2] (");
            sb.AppendLine("    [Id] int IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("    [Int2] int  NULL,");
            sb.AppendLine("    [Long2] bigint  NULL,");
            sb.AppendLine("    [Float2] real  NULL,");
            sb.AppendLine("    [Double2] float  NULL,");
            sb.AppendLine("    [Decimal2] decimal(18,2)  NULL,");
            sb.AppendLine("    [Decimal3] decimal(20,4)  NULL,");
            sb.AppendLine("    [Guid2] uniqueidentifier  NULL,");
            sb.AppendLine("    [Short2] smallint  NULL,");
            sb.AppendLine("    [DateTime2] datetime2  NULL,");
            sb.AppendLine("    [Bool2] bit  NULL,");
            sb.AppendLine("    [TimeSpan2] time  NULL,");
            sb.AppendLine("    [Byte2] tinyint  NULL,");
            sb.AppendLine("    [String2] nvarchar(100)  NULL,");
            sb.AppendLine("    [String3] nvarchar(max)  NULL,");
            sb.AppendLine("    [Enum2] int  NULL,");
            sb.AppendLine("    [TestInt3] int  NULL,");
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
            //dbGenerator.ExecuteGenerateSql(result[0]);

            sb.Clear();
            sb.AppendLine("CREATE TABLE dbo.[NotNullableTable2] (");
            sb.AppendLine("    [Id] int IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("    [Int2] int  NOT NULL,");
            sb.AppendLine("    [Long2] bigint  NOT NULL,");
            sb.AppendLine("    [Float2] real  NOT NULL,");
            sb.AppendLine("    [Double2] float  NOT NULL,");
            sb.AppendLine("    [Decimal2] decimal(18,2)  NOT NULL,");
            sb.AppendLine("    [Decimal3] decimal(20,4)  NOT NULL,");
            sb.AppendLine("    [Guid2] uniqueidentifier  NOT NULL,");
            sb.AppendLine("    [Short2] smallint  NOT NULL,");
            sb.AppendLine("    [DateTime2] datetime2  NOT NULL,");
            sb.AppendLine("    [Bool2] bit  NOT NULL,");
            sb.AppendLine("    [TimeSpan2] time  NOT NULL,");
            sb.AppendLine("    [Byte2] tinyint  NOT NULL,");
            sb.AppendLine("    [String2] nvarchar(100)  NOT NULL,");
            sb.AppendLine("    [String3] nvarchar(max)  NOT NULL,");
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
            Assert.Equal("ALTER TABLE dbo.[NullableTable] ADD [int3] int  NULL", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE dbo.[SpecifiedMapTestTable] (");
            sb.AppendLine("    [NormalTxt] nvarchar(max)  NULL,");
            sb.AppendLine("    [SpecifiedTxt] text  NULL");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
        }



        [Fact, Priority(402)]
        public void TestSqlServer()
        {
            InitDatabase();
            TestRepository();

        }

        [Fact, Priority(401)]
        public async Task TestSqlServerAsync()
        {
            InitDatabase();
            await TestRepositoryAsync();
        }

        private void InitDatabase()
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

            InitService();
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
        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址
        private void InitService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(CONFIG_FILE, true, true);
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
                it.DbConnectionType = typeof(SqlConnection);
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

            var ccc = customerRepository.Where(it => it.Name == "testCustomer").Distinct().FirstOrDefault();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single((IEnumerable)age5Customers);
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
                var orderDetail3 = new OrderDetail
                {
                    OrderHeaderId = orderHeader.Id,
                    ProductName = "ball",
                    Quantity = 3
                };
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

            //var customers= customerRepository.Where(it => it.Age > 5).OrderBy(it => it.Id).Take(10).ToList();

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
        }

        private void test(Expression<Func<Customer, object>> exp, object value)
        {

        }

        public void TestRepository()
        {
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var orderHeaderRepository = serviceProvider.GetService<IOrderHeaderRepository>();
            var orderDetailRepository = serviceProvider.GetService<IOrderDetailRepository>();

            var customCustomerRepository = serviceProvider.GetService<ICustomCustomerRepository>();

            //Test insert,update,get,delete 
            var customer = new Customer() { Name = "testCustomer" };
            customerRepository.Insert(customer);

            var updateCount = customerRepository.Where(it => it.Name == "testCustomer")
                 .SetValue(it => it.Age, 5)
                 .SetValue(it => it.TotalConsumptionAmount, 100)
                 .ExecuteUpdate();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single((IEnumerable)age5Customers);
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
            //0-99岁，大于5的只有94个
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);
            var page2 = customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPage();
            Assert.Equal(94, page2.TotalPages);
            Assert.Equal(10, page2.Data.Count);

            //测试bindWhere构造条件
            var nameEmpty = WhereBuilder.Empty<string>();//
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
            customerRepository.Delete(it => it.Age > 5);
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
