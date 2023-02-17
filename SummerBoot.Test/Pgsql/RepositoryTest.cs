﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using SummerBoot.Test.Pgsql.Db;
using SummerBoot.Test.Pgsql.Models;
using SummerBoot.Test.Pgsql.Repository;
using Xunit;
using Xunit.Priority;

namespace SummerBoot.Test.Pgsql
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RepositoryTest
    {
        private IServiceProvider serviceProvider;


        /// <summary>
        /// 测试插入实体和更新实体前的自定义函数
        /// </summary>
        [Fact, Priority(513)]
        public async Task TestBeforeInsertAndUpdateEvent()
        {
            InitDatabase();
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
        [Fact, Priority(513)]
        public async Task TestModelUseGuidAsId()
        {
            InitDatabase();
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
        [Fact, Priority(512)]
        public async Task TestBatchInsertWithDbtransation()
        {
            InitDatabase();
            var connectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("pgsql connectionString must not be null");
            }
            var guid = Guid.NewGuid();
            var now = DateTime.Now;
            var now2 = now;
            var total = 2000;
            var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
            var unitOfWork = serviceProvider.GetService<IUnitOfWork1>();
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
            var count2 = (await nullableTableRepository.GetAllAsync()).Count;
            Assert.Equal(4000, count2);
        }

        /// <summary>
        /// 测试批量插入
        /// </summary>
        //[Fact, Priority(511)]
        //public async Task TestBatchInsertAsync()
        //{
        //    InitDatabase();
        //    //InitService();
        //    var connectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
        //    if (string.IsNullOrWhiteSpace(connectionString))
        //    {
        //        throw new ArgumentNullException("pgsql connectionString must not be null");
        //    }

        //    var guid = Guid.NewGuid();
        //    var now = DateTime.Now;
        //    var now2 = now;
        //    var total = 2000;
        //    var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
        //    var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
        //    var nullableTableList3 = new List<NullableTable>();
        //    var nullableTableList = new List<NullableTable>();
        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 0,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList3.Add(a);
        //    }

        //    //预热
        //    using (var dbConnection = new MySqlConnection(connectionString))
        //    {
        //        dbConnection.Open();
        //        //var dbtran = dbConnection.BeginTransaction();
        //        //MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection,
        //        //    dbtran);
        //        MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection, null);
        //        sqlBulkCopy.DestinationTableName = "NullableTable";
        //        var propertys = typeof(NullableTable).GetProperties()
        //            .Where(it => it.CanRead && it.GetCustomAttribute<NotMappedAttribute>() == null).ToList();


        //        for (int i = 0; i < propertys.Count; i++)
        //        {
        //            var property = propertys[i];
        //            var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;


        //            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, "@tmp", $"{columnName} =unhex(@tmp)"));
        //            }
        //            else
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, columnName));
        //            }
        //        }

        //        var table = nullableTableList3.ToDataTable();

        //        SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(table, guid1 => guid1.ToByteArray());

        //        var c = await sqlBulkCopy.WriteToServerAsync(table);

        //        if (c.Warnings.Count > 1)
        //        {
        //            throw new Exception(string.Join(',', c.Warnings.Select(it => it.Message)));
        //        }
        //        //dbtran.Commit();

        //    }

        //    nullableTableRepository.Delete(it => true);
        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 1,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList.Add(a);
        //    }

        //    var l1 = await SbUtil.CalculateTimeAsync("FastBatchInsertAsync", async () =>
        //    {
        //        await nullableTableRepository.FastBatchInsertAsync(nullableTableList);
        //    });

        //    //正式开始
        //    var sw = new Stopwatch();
        //    sw.Start();

        //    using (var dbConnection = new MySqlConnection(connectionString))
        //    {
        //        dbConnection.Open();
        //        //var dbtran = dbConnection.BeginTransaction();
        //        //MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection,
        //        //    dbtran);
        //        MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection, null);
        //        sqlBulkCopy.DestinationTableName = "NullableTable";
        //        var propertys = typeof(NullableTable).GetProperties()
        //            .Where(it => it.CanRead && it.GetCustomAttribute<NotMappedAttribute>() == null).ToList();


        //        for (int i = 0; i < propertys.Count; i++)
        //        {
        //            var property = propertys[i];
        //            var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;

        //            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, "@tmp", $"{columnName} =unhex(@tmp)"));
        //            }
        //            else
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, columnName));
        //            }

        //        }

        //        var table = nullableTableList3.ToDataTable();

        //        SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(table, guid1 => guid1.ToByteArray());

        //        var cccccc = await SbUtil.CalculateTimeAsync("normalInsert", async () =>
        //        {
        //            var c = await sqlBulkCopy.WriteToServerAsync(table);
        //        });


        //        //if (c.Warnings.Count > 1)
        //        //{
        //        //    throw new Exception(string.Join(',', c.Warnings.Select(it => it.Message)));
        //        //}
        //        //dbtran.Commit();

        //    }
        //    sw.Stop();
        //    var l2 = sw.ElapsedMilliseconds;

        //    var nullableTableList2 = new List<NullableTable>();

        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now2,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 1,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList2.Add(a);
        //    }
        //    sw.Restart();
        //    await nullableTableRepository.InsertAsync(nullableTableList2);
        //    sw.Stop();
        //    var l3 = sw.ElapsedMilliseconds;

        //    sw.Restart();

        //    var rate = l1 / l2;
        //    var rate2 = l3 / l1;
        //    var rate3 = l3 / l2;

        //    var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
        //    Assert.Equal(3, result.Count);
        //    result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
        //    Assert.Equal(total * 3, result.Count);
        //    var models = nullableTableRepository.Where(it => it.Int2 == 1)
        //        .ToList();
        //    var count = nullableTableRepository.Count(it => it.Int2 == 1);
        //    Assert.Equal(3, count);
        //    Assert.Equal(3, models.Count);
        //    Assert.True(models[0].Equals(models[1]));
        //    Assert.True(models[0].Equals(models[2]));
        //}

        ///// <summary>
        ///// 测试批量插入
        ///// </summary>
        //[Fact, Priority(510)]
        //public async Task TestBatchInsert()
        //{
        //    InitDatabase();
        //    //InitService();
        //    var connectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
        //    if (string.IsNullOrWhiteSpace(connectionString))
        //    {
        //        throw new ArgumentNullException("pgsql connectionString must not be null");
        //    }

        //    var guid = Guid.NewGuid();
        //    var now = DateTime.Now;
        //    var now2 = now;
        //    var total = 2000;
        //    var nullableTableRepository = serviceProvider.GetService<INullableTableRepository>();
        //    var dbFactory = serviceProvider.GetService<IUnitOfWork1>().DbFactory;
        //    var nullableTableList3 = new List<NullableTable>();
        //    var nullableTableList = new List<NullableTable>();

        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 0,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList3.Add(a);
        //    }

        //    //预热
        //    using (var dbConnection = new MySqlConnection(connectionString))
        //    {
        //        dbConnection.Open();
        //        //var dbtran = dbConnection.BeginTransaction();
        //        //MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection,
        //        //    dbtran);
        //        MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection, null);
        //        sqlBulkCopy.DestinationTableName = "NullableTable";
        //        var propertys = typeof(NullableTable).GetProperties()
        //            .Where(it => it.CanRead && it.GetCustomAttribute<NotMappedAttribute>() == null).ToList();


        //        for (int i = 0; i < propertys.Count; i++)
        //        {
        //            var property = propertys[i];
        //            var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;


        //            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, "@tmp", $"{columnName} =unhex(@tmp)"));
        //            }
        //            else
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, columnName));
        //            }
        //        }

        //        var table = nullableTableList3.ToDataTable();

        //        SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(table, guid1 => guid1.ToByteArray());

        //        var c = sqlBulkCopy.WriteToServer(table);

        //        if (c.Warnings.Count > 1)
        //        {
        //            throw new Exception(string.Join(',', c.Warnings.Select(it => it.Message)));
        //        }
        //        //dbtran.Commit();

        //    }

        //    nullableTableRepository.Delete(it => true);
        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 1,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList.Add(a);
        //    }

        //    var l1 = SbUtil.CalculateTime("FastBatchInsert", (() =>
        //    {
        //        nullableTableRepository.FastBatchInsert(nullableTableList);
        //    }));
        //    //正式开始
        //    var sw = new Stopwatch();
        //    sw.Start();

        //    using (var dbConnection = new MySqlConnection(connectionString))
        //    {
        //        dbConnection.Open();
        //        //var dbtran = dbConnection.BeginTransaction();
        //        //MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection,
        //        //    dbtran);
        //        MySqlBulkCopy sqlBulkCopy = new MySqlBulkCopy(dbConnection, null);
        //        sqlBulkCopy.DestinationTableName = "NullableTable";
        //        var propertys = typeof(NullableTable).GetProperties()
        //            .Where(it => it.CanRead && it.GetCustomAttribute<NotMappedAttribute>() == null).ToList();


        //        for (int i = 0; i < propertys.Count; i++)
        //        {
        //            var property = propertys[i];
        //            var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;

        //            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, "@tmp", $"{columnName} =unhex(@tmp)"));
        //            }
        //            else
        //            {
        //                sqlBulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, columnName));
        //            }

        //        }

        //        var table = nullableTableList3.ToDataTable();

        //        SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(table, guid1 => guid1.ToByteArray());
        //        SbUtil.CalculateTime("zs", () =>
        //        {
        //            var c = sqlBulkCopy.WriteToServer(table);
        //        });

        //        //if (c.Warnings.Count > 1)
        //        //{
        //        //    throw new Exception(string.Join(',', c.Warnings.Select(it => it.Message)));
        //        //}
        //        //dbtran.Commit();

        //    }
        //    sw.Stop();
        //    var l2 = sw.ElapsedMilliseconds;

        //    var nullableTableList2 = new List<NullableTable>();

        //    for (int i = 0; i < total; i++)
        //    {
        //        var a = new NullableTable()
        //        {
        //            Int2 = 2,
        //            Bool2 = true,
        //            Byte2 = 1,
        //            DateTime2 = now2,
        //            Decimal2 = 1m,
        //            Decimal3 = 1.1m,
        //            Double2 = 1.1,
        //            Float2 = (float)1.1,
        //            Guid2 = Guid.NewGuid(),
        //            Id = 1,
        //            Short2 = 1,
        //            TimeSpan2 = TimeSpan.FromHours(1),
        //            String2 = "sb",
        //            String3 = "sb",
        //            Long2 = 2,
        //            Enum2 = Model.Enum2.y,
        //            Int3 = 4
        //        };
        //        if (i == 0)
        //        {
        //            a.Guid2 = guid;
        //            a.Int2 = 1;
        //        }
        //        nullableTableList2.Add(a);
        //    }
        //    sw.Restart();
        //    nullableTableRepository.Insert(nullableTableList2);
        //    sw.Stop();
        //    var l3 = sw.ElapsedMilliseconds;

        //    sw.Restart();

        //    var rate = l1 / l2;
        //    var rate2 = l3 / l1;
        //    var rate3 = l3 / l2;

        //    var result = nullableTableRepository.Where(it => it.Guid2 == guid).OrderBy(it => it.Id).ToList();
        //    Assert.Equal(3, result.Count);
        //    result = nullableTableRepository.Where(it => it.Enum2 == Model.Enum2.y).OrderBy(it => it.Id).ToList();
        //    Assert.Equal(total * 3, result.Count);
        //    var models = nullableTableRepository.Where(it => it.Int2 == 1)
        //        .ToList();
        //    var count = nullableTableRepository.Count(it => it.Int2 == 1);
        //    Assert.Equal(3, count);
        //    Assert.Equal(3, models.Count);
        //    Assert.True(models[0].Equals(models[1]));
        //    Assert.True(models[0].Equals(models[2]));
        //}

        /// <summary>
        /// 测试从配置文件读取sql
        /// </summary>
        [Fact, Priority(509)]
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
        [Fact, Priority(508)]
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
        [Fact, Priority(507)]
        public void TestTableSchemaAndAddPrimaryKey()
        {
            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var customerWithSchema2Repository = serviceProvider.GetService<ICustomerWithSchema2Repository>();
            var sb = new StringBuilder();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema) });
            sb.Clear();
            sb.AppendLine("CREATE TABLE test1.\"customerwithschema\" (");
            sb.AppendLine("    \"name\" text NULL ,");
            sb.AppendLine("    \"age\" int4 NOT NULL ,");
            sb.AppendLine("    \"customerno\" text NULL ,");
            sb.AppendLine("    \"totalconsumptionamount\" numeric(18,2) NOT NULL ");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
            foreach (var generateDatabaseSqlResult in result)
            {
                dbGenerator.ExecuteGenerateSql(generateDatabaseSqlResult);
            }
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(CustomerWithSchema2) });
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"ID\" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY", result[0].FieldModifySqls[0]);
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"LASTUPDATEON\" timestamp NULL ", result[0].FieldModifySqls[1]);
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"LASTUPDATEBY\" text NULL ", result[0].FieldModifySqls[2]);
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"CREATEON\" timestamp NULL ", result[0].FieldModifySqls[3]);
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"CREATEBY\" text NULL ", result[0].FieldModifySqls[4]);
            Assert.Equal("ALTER TABLE test1.\"customerwithschema\" ADD \"ACTIVE\" int4 NULL ", result[0].FieldModifySqls[5]);
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

        [Fact, Priority(506)]
        public void TestCreateTableFromEntityAndCrud()
        {
            InitDatabase();
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
        [Fact, Priority(505)]
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

        [Fact, Priority(504)]
        public void TestGenerateCsharpClassByDatabaseInfo()
        {
            InitDatabase();

            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();
            var result = dbGenerator.GenerateCsharpClass(new List<string>() { "customer", "nullabletable", "notnullabletable" }, "abc");
            Assert.Equal(3, result.Count);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("namespace abc");
            sb.AppendLine("{");
            sb.AppendLine("   [Table(\"customer\")]");
            sb.AppendLine("   public class Customer");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      [Column(\"name\")]");
            sb.AppendLine("      public string Name { get; set; }");
            sb.AppendLine("      [Column(\"age\")]");
            sb.AppendLine("      public int Age { get; set; }");
            sb.AppendLine("      [Column(\"customerno\")]");
            sb.AppendLine("      public string Customerno { get; set; }");
            sb.AppendLine("      [Column(\"totalconsumptionamount\")]");
            sb.AppendLine("      public decimal Totalconsumptionamount { get; set; }");
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
            sb.AppendLine("   [Table(\"nullabletable\")]");
            sb.AppendLine("   public class Nullabletable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"int2\")]");
            sb.AppendLine("      public int? Int2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"long2\")]");
            sb.AppendLine("      public long? Long2 { get; set; }");
            sb.AppendLine("      [Column(\"float2\")]");
            sb.AppendLine("      public float? Float2 { get; set; }");
            sb.AppendLine("      [Column(\"double2\")]");
            sb.AppendLine("      public double? Double2 { get; set; }");
            sb.AppendLine("      [Column(\"decimal2\")]");
            sb.AppendLine("      public decimal? Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"decimal3\")]");
            sb.AppendLine("      public decimal? Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"guid2\")]");
            sb.AppendLine("      public Guid? Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"short2\")]");
            sb.AppendLine("      public short? Short2 { get; set; }");
            sb.AppendLine("      [Column(\"datetime2\")]");
            sb.AppendLine("      public DateTime? Datetime2 { get; set; }");
            sb.AppendLine("      [Column(\"bool2\")]");
            sb.AppendLine("      public bool? Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"timespan2\")]");
            sb.AppendLine("      public TimeSpan? Timespan2 { get; set; }");
            sb.AppendLine("      [Column(\"byte2\")]");
            sb.AppendLine("      public short? Byte2 { get; set; }");
            sb.AppendLine("      [Column(\"string2\")]");
            sb.AppendLine("      public string String2 { get; set; }");
            sb.AppendLine("      [Column(\"string3\")]");
            sb.AppendLine("      public string String3 { get; set; }");
            sb.AppendLine("      [Column(\"enum2\")]");
            sb.AppendLine("      public int? Enum2 { get; set; }");
            sb.AppendLine("      [Column(\"testint3\")]");
            sb.AppendLine("      public int? Testint3 { get; set; }");
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
            sb.AppendLine("   [Table(\"notnullabletable\")]");
            sb.AppendLine("   public class Notnullabletable");
            sb.AppendLine("   {");
            sb.AppendLine("      [Key]");
            sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            sb.AppendLine("      [Column(\"id\")]");
            sb.AppendLine("      public int Id { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Int2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"int2\")]");
            sb.AppendLine("      public int Int2 { get; set; }");
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      ///Long2");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      [Column(\"long2\")]");
            sb.AppendLine("      public long Long2 { get; set; }");
            sb.AppendLine("      [Column(\"float2\")]");
            sb.AppendLine("      public float Float2 { get; set; }");
            sb.AppendLine("      [Column(\"double2\")]");
            sb.AppendLine("      public double Double2 { get; set; }");
            sb.AppendLine("      [Column(\"decimal2\")]");
            sb.AppendLine("      public decimal Decimal2 { get; set; }");
            sb.AppendLine("      [Column(\"decimal3\")]");
            sb.AppendLine("      public decimal Decimal3 { get; set; }");
            sb.AppendLine("      [Column(\"guid2\")]");
            sb.AppendLine("      public Guid Guid2 { get; set; }");
            sb.AppendLine("      [Column(\"short2\")]");
            sb.AppendLine("      public short Short2 { get; set; }");
            sb.AppendLine("      [Column(\"datetime2\")]");
            sb.AppendLine("      public DateTime Datetime2 { get; set; }");
            sb.AppendLine("      [Column(\"bool2\")]");
            sb.AppendLine("      public bool Bool2 { get; set; }");
            sb.AppendLine("      [Column(\"timespan2\")]");
            sb.AppendLine("      public TimeSpan Timespan2 { get; set; }");
            sb.AppendLine("      [Column(\"byte2\")]");
            sb.AppendLine("      public short Byte2 { get; set; }");
            sb.AppendLine("      [Column(\"string2\")]");
            sb.AppendLine("      public string String2 { get; set; }");
            sb.AppendLine("      [Column(\"string3\")]");
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
        [Fact, Priority(503)]
        public void TestGenerateDatabaseTableByCsharpClass()
        {

            InitDatabase();
            var dbGenerator = serviceProvider.GetService<IDbGenerator1>();

            var result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable2), typeof(NotNullableTable2) });
            Assert.Equal(2, result.Count());
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE public.\"nullabletable2\" (");
            sb.AppendLine("    \"id\" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY,");
            sb.AppendLine("    \"int2\" int4 NULL ,");
            sb.AppendLine("    \"long2\" int8 NULL ,");
            sb.AppendLine("    \"float2\" float4 NULL ,");
            sb.AppendLine("    \"double2\" float8 NULL ,");
            sb.AppendLine("    \"decimal2\" numeric(18,2) NULL ,");
            sb.AppendLine("    \"decimal3\" numeric(20,4) NULL ,");
            sb.AppendLine("    \"guid2\" uuid NULL ,");
            sb.AppendLine("    \"short2\" int2 NULL ,");
            sb.AppendLine("    \"datetime2\" timestamp NULL ,");
            sb.AppendLine("    \"bool2\" bool NULL ,");
            sb.AppendLine("    \"timespan2\" interval NULL ,");
            sb.AppendLine("    \"byte2\" int2 NULL ,");
            sb.AppendLine("    \"string2\" varchar(100) NULL ,");
            sb.AppendLine("    \"string3\" text NULL ,");
            sb.AppendLine("    \"enum2\" int4 NULL ,");
            sb.AppendLine("    \"testint3\" int4 NULL ,");
            sb.AppendLine(" CONSTRAINT nullabletable2_pk PRIMARY KEY (id)");
            sb.AppendLine(")");
            var exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);

            Assert.Equal(4, result[0].Descriptions.Count);
            Assert.Equal("COMMENT ON TABLE public.\"nullabletable2\" IS 'NullableTable2'", result[0].Descriptions[0]);
            Assert.Equal("COMMENT ON COLUMN public.\"nullabletable2\".\"int2\" IS 'Int2'", result[0].Descriptions[1]);
            Assert.Equal("COMMENT ON COLUMN public.\"nullabletable2\".\"long2\" IS 'Long2'", result[0].Descriptions[2]);
            Assert.Equal("COMMENT ON COLUMN public.\"nullabletable2\".\"testint3\" IS 'Int2'", result[0].Descriptions[3]);
            dbGenerator.ExecuteGenerateSql(result[0]);

            sb.Clear();
            sb.AppendLine("CREATE TABLE public.\"notnullabletable2\" (");
            sb.AppendLine("    \"id\" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY,");
            sb.AppendLine("    \"int2\" int4 NOT NULL ,");
            sb.AppendLine("    \"long2\" int8 NOT NULL ,");
            sb.AppendLine("    \"float2\" float4 NOT NULL ,");
            sb.AppendLine("    \"double2\" float8 NOT NULL ,");
            sb.AppendLine("    \"decimal2\" numeric(18,2) NOT NULL ,");
            sb.AppendLine("    \"decimal3\" numeric(20,4) NOT NULL ,");
            sb.AppendLine("    \"guid2\" uuid NOT NULL ,");
            sb.AppendLine("    \"short2\" int2 NOT NULL ,");
            sb.AppendLine("    \"datetime2\" timestamp NOT NULL ,");
            sb.AppendLine("    \"bool2\" bool NOT NULL ,");
            sb.AppendLine("    \"timespan2\" interval NOT NULL ,");
            sb.AppendLine("    \"byte2\" int2 NOT NULL ,");
            sb.AppendLine("    \"string2\" varchar(100) NOT NULL ,");
            sb.AppendLine("    \"string3\" text NOT NULL ,");
            sb.AppendLine(" CONSTRAINT notnullabletable2_pk PRIMARY KEY (id)");
            sb.AppendLine(")");

            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[1].Body);

            sb.Clear();
            result = dbGenerator.GenerateSql(new List<Type>() { typeof(NullableTable3) });
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result[0].Descriptions.Count);
            Assert.Equal("COMMENT ON COLUMN public.\"nullabletable\".\"int3\" IS 'test add column'", result[0].Descriptions[0]);
            Assert.Equal(1, result[0].FieldModifySqls.Count);
            Assert.Equal("ALTER TABLE public.\"nullabletable\" ADD \"int3\" int4 NULL ", result[0].FieldModifySqls[0]);

            result = dbGenerator.GenerateSql(new List<Type>() { typeof(SpecifiedMapTestTable) });
            Assert.Equal(1, result.Count());
            sb.Clear();
            sb.AppendLine("CREATE TABLE public.\"specifiedmaptesttable\" (");
            sb.AppendLine("    \"normaltxt\" text NULL ,");
            sb.AppendLine("    \"specifiedtxt\" CLOB NULL ");
            sb.AppendLine(")");
            exceptStr = sb.ToString();
            Assert.Equal(exceptStr
                , result[0].Body);
        }

        [Fact, Priority(502)]
        public void TestPgsql()
        {
            InitDatabase();
            TestRepository();

        }

        [Fact, Priority(501)]
        public async Task TestPgsqlAsync()
        {
            InitDatabase();
            await TestRepositoryAsync();
        }

        private void InitDatabase()
        {
            //初始化数据库
            using (var database = new PgsqlDb())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
                database.Database.ExecuteSqlRaw(
                    "create schema test1");
                //database.Database.ExecuteSqlRaw(
                //    "ALTER TABLE nullabletable MODIFY COLUMN `Int2` int NULL COMMENT 'Int2'");
                //database.Database.ExecuteSqlRaw(
                //    "ALTER TABLE nullabletable MODIFY COLUMN `Long2` bigint NULL COMMENT 'Long2'");

                //database.Database.ExecuteSqlRaw(
                //    "ALTER TABLE notnullabletable COMMENT = 'NotNullableTable'");
                //database.Database.ExecuteSqlRaw(
                //    "ALTER TABLE notnullabletable MODIFY COLUMN `Int2` int not NULL COMMENT 'Int2'");
                //database.Database.ExecuteSqlRaw(
                //    "ALTER TABLE notnullabletable MODIFY COLUMN `Long2` bigint not NULL COMMENT 'Long2'");
                //try
                //{
                //    database.Database.ExecuteSqlRaw(
                //        "drop TABLE test1.`CustomerWithSchema`");
                //}
                //catch (Exception e)
                //{

                //}
            }
            InitService();
        }
        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址
        private void InitService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(CONFIG_FILE, true, true);
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
                        x.BindRepositorysWithAttribute<PgsqlAutoRepositoryAttribute>();
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
            customerRepository.Delete(it => it.Age > 5);
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