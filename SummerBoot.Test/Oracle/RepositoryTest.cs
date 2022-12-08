using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Oracle.Db;
using SummerBoot.Test.Oracle.Models;
using SummerBoot.Test.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.TypeHandler;
using SummerBoot.Test.Model;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.Oracle
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 测试id类型为guid的model的增删改查
        /// </summary>
        [Fact, Priority(214)]
        public async Task TestModelUseGuidAsId()
        {
            InitOracleDatabase();
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
        /// 测试事务中批量插入
        /// </summary>
        [Fact, Priority(213)]
        public async Task TestBatchInsertWithDbtransation()
        {
            InitOracleDatabase();
         
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
     
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
            var count2 = (await nullableTableRepository.GetAllAsync()).Count;
            Assert.Equal(4000, count2);
        }
        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(212)]
        public async Task TestBatchInsertAsync()
        {
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitOracleDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();
            sw.Start();
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
                    Enum2 = Model.Enum2.y
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            await nullableTableRepository.FastBatchInsertAsync(nullableTableList);

            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;

            sw.Restart();
            await nullableTableRepository.InsertAsync(nullableTableList);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;

            sw.Restart();
            //Customer
            var connection = dbFactory.GetDbConnection() as OracleConnection;
            //connection.Open();
            int?[] Int2 = new int?[total];
            bool[] Bool2 = new bool[total];
            byte[] Byte2 = new byte[total];
            DateTime[] DateTime2 = new DateTime[total];
            decimal?[] Decimal2 = new decimal?[total];
            decimal[] Decimal3 = new decimal[total];
            double[] Double2 = new double[total];
            float[] Float2 = new float[total];
            Guid?[] Guid2 = new Guid?[total];
            short[] Short2 = new short[total];
            TimeSpan[] TimeSpan2 = new TimeSpan[total];
            string[] String2 = new string[total];
            string[] String3 = new string[total];
            long[] Long2 = new long[total];
            Enum2[] Enum2 = new Enum2[total];

            for (int j = 0; j < total; j++)
            {
                Int2[j] = 2;
                Bool2[j] = true;
                Byte2[j] = 1;
                DateTime2[j] = now2;
                Decimal2[j] = 1m;
                Decimal3[j] = 1.1m;
                Double2[j] = 1.1;
                Float2[j] = (float)1.1;
                Guid2[j] = Guid.NewGuid();
                Short2[j] = 1;
                TimeSpan2[j] = TimeSpan.FromHours(1);
                String2[j] = "sb";
                String3[j] = "sb";
                Long2[j] = 2;
                Enum2[j] = Model.Enum2.y;
                if (j == 0)
                {
                    Guid2[j] = guid;
                }
            }

            var c = (int)Model.Enum2.y;
            OracleParameter pInt2 = new OracleParameter();
            pInt2.OracleDbType = OracleDbType.Int32;
            pInt2.Value = Int2;

            OracleParameter pBool2 = new OracleParameter();
            pBool2.OracleDbType = OracleDbType.Byte;
            pBool2.Value = Bool2;

            OracleParameter pByte2 = new OracleParameter();
            pByte2.OracleDbType = OracleDbType.Byte;
            pByte2.Value = Byte2;

            OracleParameter pDateTime2 = new OracleParameter();
            pDateTime2.OracleDbType = OracleDbType.TimeStamp;
            pDateTime2.Value = DateTime2;

            OracleParameter pDecimal2 = new OracleParameter();
            pDecimal2.OracleDbType = OracleDbType.Decimal;
            pDecimal2.Value = Decimal2;

            OracleParameter pDecimal3 = new OracleParameter();
            pDecimal3.OracleDbType = OracleDbType.Decimal;
            pDecimal3.Value = Decimal3;

            OracleParameter pDouble2 = new OracleParameter();
            pDouble2.OracleDbType = OracleDbType.Double;
            pDouble2.Value = Double2;

            OracleParameter pFloat2 = new OracleParameter();
            pFloat2.OracleDbType = OracleDbType.BinaryFloat;
            pFloat2.Value = Float2;


            OracleParameter pGuid2 = new OracleParameter();
            pGuid2.OracleDbType = OracleDbType.Raw;
            pGuid2.Value = Guid2;

            OracleParameter pShort2 = new OracleParameter();
            pShort2.OracleDbType = OracleDbType.Int16;
            pShort2.Value = Short2;

            OracleParameter pTimeSpan2 = new OracleParameter();
            pTimeSpan2.OracleDbType = OracleDbType.IntervalDS;
            pTimeSpan2.Value = TimeSpan2;

            OracleParameter pString2 = new OracleParameter();
            pString2.OracleDbType = OracleDbType.Varchar2;
            pString2.Value = String2;

            OracleParameter pString3 = new OracleParameter();
            pString3.OracleDbType = OracleDbType.Varchar2;
            pString3.Value = String3;


            OracleParameter pLong2 = new OracleParameter();
            pLong2.OracleDbType = OracleDbType.Long;
            pLong2.Value = Long2;

            OracleParameter pEnum2 = new OracleParameter();
            pEnum2.OracleDbType = OracleDbType.Byte;
            pEnum2.Value = Enum2;
            // create command and set properties
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO NULLABLETABLE (INT2, LONG2, FLOAT2, DOUBLE2, DECIMAL2, DECIMAL3, GUID2, SHORT2, DATETIME2, BOOL2, TIMESPAN2, BYTE2, STRING2, STRING3,ENUM2) VALUES(:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15)";
            cmd.ArrayBindCount = total;
            cmd.Parameters.Add(pInt2);
            cmd.Parameters.Add(pLong2);
            cmd.Parameters.Add(pFloat2);
            cmd.Parameters.Add(pDouble2);
            cmd.Parameters.Add(pDecimal2);
            cmd.Parameters.Add(pDecimal3);
            cmd.Parameters.Add(pGuid2);
            cmd.Parameters.Add(pShort2);
            cmd.Parameters.Add(pDateTime2);
            cmd.Parameters.Add(pBool2);
            cmd.Parameters.Add(pTimeSpan2);
            cmd.Parameters.Add(pByte2);
            cmd.Parameters.Add(pString2);
            cmd.Parameters.Add(pString3);
            cmd.Parameters.Add(pEnum2);
            await cmd.ExecuteNonQueryAsync();
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
            var count = nullableTableRepository.Count(it => it.Guid2 == guid);
            Assert.Equal(3, count);
            Assert.Equal(3, result.Count);
            result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        [Fact, Priority(211)]
        public async Task TestBatchInsert()
        {
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            InitOracleDatabase();
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
            var sw = new Stopwatch();
            var nullableTableList = new List<NullableTable>();
            sw.Start();
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
                    Enum2 = Model.Enum2.y
                };
                if (i == 0)
                {
                    a.Guid2 = guid;
                }
                nullableTableList.Add(a);
            }

            nullableTableRepository.FastBatchInsert(nullableTableList);
           
            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
           
            sw.Restart();
            nullableTableRepository.Insert(nullableTableList);
            sw.Stop();
            var l3 = sw.ElapsedMilliseconds;

            sw.Restart();
            //Customer
            var connection = dbFactory.GetDbConnection() as OracleConnection;
            //connection.Open();
            int?[] Int2 = new int?[total];
            bool[] Bool2 = new bool[total];
            byte[] Byte2 = new byte[total];
            DateTime[] DateTime2 = new DateTime[total];
            decimal?[] Decimal2 = new decimal?[total];
            decimal[] Decimal3 = new decimal[total];
            double[] Double2 = new double[total];
            float[] Float2 = new float[total];
            Guid?[] Guid2 = new Guid?[total];
            short[] Short2 = new short[total];
            TimeSpan[] TimeSpan2 = new TimeSpan[total];
            string[] String2 = new string[total];
            string[] String3 = new string[total];
            long[] Long2 = new long[total];
            Enum2[] Enum2 = new Enum2[total];

            for (int j = 0; j < total; j++)
            {
                Int2[j] = 2;
                Bool2[j] = true;
                Byte2[j] = 1;
                DateTime2[j] = now2;
                Decimal2[j] = 1m;
                Decimal3[j] = 1.1m;
                Double2[j] = 1.1;
                Float2[j] = (float)1.1;
                Guid2[j] = Guid.NewGuid();
                Short2[j] = 1;
                TimeSpan2[j] = TimeSpan.FromHours(1);
                String2[j] = "sb";
                String3[j] = "sb";
                Long2[j] = 2;
                Enum2[j] = Model.Enum2.y;
                if (j == 0)
                {
                    Guid2[j] = guid;
                }
            }

            OracleParameter pInt2 = new OracleParameter();
            pInt2.OracleDbType = OracleDbType.Int32;
            pInt2.Value = Int2;

            OracleParameter pBool2 = new OracleParameter();
            pBool2.OracleDbType = OracleDbType.Byte;
            pBool2.Value = Bool2;
            
            OracleParameter pByte2 = new OracleParameter();
            pByte2.OracleDbType = OracleDbType.Byte;
            pByte2.Value = Byte2;

            OracleParameter pDateTime2 = new OracleParameter();
            pDateTime2.OracleDbType = OracleDbType.TimeStamp;
            pDateTime2.Value = DateTime2;

            OracleParameter pDecimal2 = new OracleParameter();
            pDecimal2.OracleDbType = OracleDbType.Decimal;
            pDecimal2.Value = Decimal2;

            OracleParameter pDecimal3 = new OracleParameter();
            pDecimal3.OracleDbType = OracleDbType.Decimal;
            pDecimal3.Value = Decimal3;

            OracleParameter pDouble2 = new OracleParameter();
            pDouble2.OracleDbType = OracleDbType.Double;
            pDouble2.Value = Double2;

            OracleParameter pFloat2 = new OracleParameter();
            pFloat2.OracleDbType = OracleDbType.BinaryFloat;
            pFloat2.Value = Float2;

            
            OracleParameter pGuid2 = new OracleParameter();
            pGuid2.OracleDbType = OracleDbType.Raw;
            pGuid2.Value = Guid2;

            OracleParameter pShort2 = new OracleParameter();
            pShort2.OracleDbType = OracleDbType.Int16;
            pShort2.Value = Short2;

            OracleParameter pTimeSpan2 = new OracleParameter();
            pTimeSpan2.OracleDbType = OracleDbType.IntervalDS;
            pTimeSpan2.Value = TimeSpan2;

            OracleParameter pString2 = new OracleParameter();
            pString2.OracleDbType = OracleDbType.Varchar2;
            pString2.Value = String2;

            OracleParameter pString3 = new OracleParameter();
            pString3.OracleDbType = OracleDbType.Varchar2;
            pString3.Value = String3;


            OracleParameter pLong2 = new OracleParameter();
            pLong2.OracleDbType = OracleDbType.Long;
            pLong2.Value = Long2;

            OracleParameter pEnum2 = new OracleParameter();
            pEnum2.OracleDbType = OracleDbType.Byte;
            pEnum2.Value = Enum2;
            // create command and set properties
            OracleCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO NULLABLETABLE (INT2, LONG2, FLOAT2, DOUBLE2, DECIMAL2, DECIMAL3, GUID2, SHORT2, DATETIME2, BOOL2, TIMESPAN2, BYTE2, STRING2, STRING3,ENUM2) VALUES(:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15)";
            cmd.ArrayBindCount = total;
            cmd.Parameters.Add(pInt2);
            cmd.Parameters.Add(pLong2);
            cmd.Parameters.Add(pFloat2);
            cmd.Parameters.Add(pDouble2);
            cmd.Parameters.Add(pDecimal2);
            cmd.Parameters.Add(pDecimal3);
            cmd.Parameters.Add(pGuid2);
            cmd.Parameters.Add(pShort2);
            cmd.Parameters.Add(pDateTime2);
            cmd.Parameters.Add(pBool2);
            cmd.Parameters.Add(pTimeSpan2);
            cmd.Parameters.Add(pByte2);
            cmd.Parameters.Add(pString2);
            cmd.Parameters.Add(pString3);
            cmd.Parameters.Add(pEnum2);
            cmd.ExecuteNonQuery();
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var rate = l1 / l2;
            var rate2 = l3 / l1;
            var rate3 = l3 / l2;
            var result= nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it=>it.Id).ToList();
            var count = nullableTableRepository.Count(it=>it.Guid2 == guid);
            Assert.Equal(3, count);
            Assert.Equal(3,result.Count);
             result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
            Assert.Equal(6000, result.Count);
        }

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(210)]
        public async Task TestGetSqlByConfigurationAsync()
        {
            InitOracleDatabase();
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
            var dbCustomer =  testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Equal(7, dbCustomer.Age);
            await testConfigurationRepository.DeleteByNameAsync("sb");
            var dbCustomer2 =  testConfigurationRepository.FirstOrDefault(it => it.Name == "sb");
            Assert.Null(dbCustomer2);
        }

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(209)]
        public void TestGetSqlByConfiguration()
        {
            InitOracleDatabase();
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
        [Fact, Priority(208)]
        public void TestTableSchemaAndAddPrimaryKey()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var customerWithSchema2Repository = serviceProvider.GetService<ICustomerWithSchema2Repository>();
            var sb = new StringBuilder();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema) });
            sb.Clear();
            sb.AppendLine("CREATE TABLE TEST1.\"CUSTOMERWITHSCHEMA\" (");
            sb.AppendLine("    \"NAME\" NVARCHAR2(2000),");
            sb.AppendLine("    \"AGE\" NUMBER(10,0) NOT NULL,");
            sb.AppendLine("    \"CUSTOMERNO\" NVARCHAR2(2000),");
            sb.AppendLine("    \"TOTALCONSUMPTIONAMOUNT\" NUMBER(18,2) NOT NULL");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema2) });
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"ID\" NUMBER(10,0) GENERATED BY DEFAULT ON NULL AS IDENTITY MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE  NOKEEP  NOSCALE NOT NULL"
                , result[0].FieldModifySqls[0]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD CONSTRAINT CUSTOMERWITHSCHEMA_PK PRIMARY KEY(ID) ENABLE"
                , result[0].FieldModifySqls[1]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"LASTUPDATEON\" TIMESTAMP(7)"
                , result[0].FieldModifySqls[2]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"LASTUPDATEBY\" NVARCHAR2(2000)"
                , result[0].FieldModifySqls[3]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"CREATEON\" TIMESTAMP(7)"
                , result[0].FieldModifySqls[4]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"CREATEBY\" NVARCHAR2(2000)"
                , result[0].FieldModifySqls[5]);
            Assert.Equal("ALTER TABLE TEST1.\"CUSTOMERWITHSCHEMA\" ADD \"ACTIVE\" NUMBER(10,0)"
                , result[0].FieldModifySqls[6]);
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
        /// ????????????????????????в?????????
        /// </summary>
        [Fact, Priority(207)]
        public void TestCreateTableFromEntityAndCrud()
        {
            InitOracleDatabase();

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
        /// ??????????
        /// </summary>
        [Fact, Priority(206)]
        public void TestTypeHandler()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var testTypeHandlerTableRepository = serviceProvider.GetService<ITestTypeHandlerTableRepository>();
            var sqls = dbGenerator.GenerateSql(new List<Type>() { typeof(TestTypeHandlerTable) });
            foreach (var generateDatabaseSqlResult in sqls)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            Guid guid = Guid.NewGuid();

            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = true, Name = "true", NullableBoolColumn = true, GuidColumn = guid, NullableGuidColumn = guid });
            var testTypeHandlerTableTrue = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "true");
            Assert.Equal(true, testTypeHandlerTableTrue.BoolColumn);
            Assert.Equal(true, testTypeHandlerTableTrue.NullableBoolColumn);
            Assert.Equal(guid, testTypeHandlerTableTrue.GuidColumn);
            Assert.Equal(guid, testTypeHandlerTableTrue.NullableGuidColumn);
            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = false, Name = "false", NullableBoolColumn = false });
            var testTypeHandlerTableFalse = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "false");
            Assert.Equal(false, testTypeHandlerTableFalse.BoolColumn);
            Assert.Equal(false, testTypeHandlerTableFalse.NullableBoolColumn);
            Assert.Equal(Guid.Empty, testTypeHandlerTableFalse.GuidColumn);
            Assert.Equal(null, testTypeHandlerTableFalse.NullableGuidColumn);
            testTypeHandlerTableRepository.Insert(new TestTypeHandlerTable() { BoolColumn = false, Name = "falseAndNull", NullableBoolColumn = null, GuidColumn = guid, NullableGuidColumn = null });
            var testTypeHandlerTableFalseAndNull = testTypeHandlerTableRepository.FirstOrDefault(it => it.Name == "falseAndNull");
            Assert.Equal(false, testTypeHandlerTableFalseAndNull.BoolColumn);
            Assert.Equal(null, testTypeHandlerTableFalseAndNull.NullableBoolColumn);
            Assert.Equal(guid, testTypeHandlerTableFalseAndNull.GuidColumn);
            Assert.Equal(null, testTypeHandlerTableFalseAndNull.NullableGuidColumn);
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        [Fact, Priority(205)]
        public void TestTableColumnMap()
        {
            InitOracleDatabase();
            var customerRepository = serviceProvider.GetService<ICustomerRepository>();
            var tableColumnMapRepository = serviceProvider.GetService<ITableColumnMapRepository>();
            customerRepository.Insert(new Customer() { Name = "sb" });
            var customer = tableColumnMapRepository.FirstOrDefault(it => it.CustomerName == "sb");
            Assert.NotNull(customer);
            Assert.Equal("sb", customer.CustomerName);
        }

        [Fact, Priority(204)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
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

        /// <summary>
        /// ???????c#????????????
        /// </summary>
        [Fact, Priority(203)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {
            InitOracleDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
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
            //dbGenerator.ExecuteGenerateSql(result[0]);

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
            sb.AppendLine("    \"SPECIFIEDTXT\" CLOB");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

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

            InitService();
        }

        [Fact, Priority(202)]
        public void TestOracle()
        {
            InitOracleDatabase();
            TestRepository();
        }

        [Fact, Priority(201)]
        public async Task TestOracleAsync()
        {
            InitOracleDatabase();
            await TestRepositoryAsync();
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
                        x.BindIRepositoryTypeWithAttribute<OracleAutoRepositoryAttribute>();
                        x.BindDbGeneratorType<IDbGenerator1>();
                    });
            });

            serviceProvider = services.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
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
            //0-99??????5?????94??
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);
            var page2 = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();
            Assert.Equal(94, page2.TotalPages);
            Assert.Equal(10, page2.Data.Count);
            //????bindWhere????????
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
            var newCount3 = await customerRepository.DeleteAsync(it => it.Age > 5);
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
       

            customerRepository.Where(it => it.Name == "testCustomer")
                .SetValue(it => it.Age, 5)
                .SetValue(it => it.TotalConsumptionAmount, 100)
                .ExecuteUpdate();

            var age5Customers = customerRepository.Where(it => it.Name == "testCustomer").ToList();
            Assert.Single(age5Customers);
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
            //0-99??????5?????94??
            Assert.Equal(94, page.TotalPages);
            Assert.Equal(10, page.Data.Count);
            var page2 = customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPage();
            Assert.Equal(94, page2.TotalPages);
            Assert.Equal(10, page2.Data.Count);

            //????bindWhere????????
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
