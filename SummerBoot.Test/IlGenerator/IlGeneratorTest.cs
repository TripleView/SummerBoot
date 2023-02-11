using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MySql.Data.MySqlClient;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Test.IlGenerator.Dto;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Type;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using SummerBoot.Repository;
using SummerBoot.Test.Model;
using IlPerson = SummerBoot.Test.IlGenerator.Dto.IlPerson;
using Xunit.Priority;

namespace SummerBoot.Test.IlGenerator
{
    [Collection("test")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class IlGeneratorTest
    {
        /// <summary>
        /// 查询多结果集
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1001)]
        public void TestQueryMulti()
        {
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
    
            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE TestQueryMulti";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE TestQueryMulti(
                Id INT auto_increment NOT null,
                Name varchar(100) NULL,
                Age INT NULL,
CONSTRAINT TestQueryMulti_pk PRIMARY KEY (Id)
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO TestQueryMulti(Name,Age) VALUES ('何泽平',30);Select LAST_INSERT_ID() id";
                var gridReader = conn.QueryMultiple(databaseUnit, insertSql);
                var id= gridReader.Read<int>().FirstOrDefault();
                Assert.Equal(1, id);

                using (var grid = conn.QueryMultiple(databaseUnit,"select 1; select 2; select @x; select 4", new { x = 3 }))
                {
                    var a = grid.Read<int>();
                    var b = grid.Read<int>();
                    var c = grid.Read<int>();
                    var d = grid.Read<int>();

                    Assert.Equal(1, a.Single());
                    Assert.Equal(2, b.Single());
                    Assert.Equal(3, c.Single());
                    Assert.Equal(4, d.Single());
                }
               
            }

          
        }

        /// <summary>
        /// 异步查询多结果集
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1002)]
        public async Task TestQueryMultiAsync()
        {
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);

            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE TestQueryMulti";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE TestQueryMulti(
                Id INT auto_increment NOT null,
                Name varchar(100) NULL,
                Age INT NULL,
CONSTRAINT TestQueryMulti_pk PRIMARY KEY (Id)
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO TestQueryMulti(Name,Age) VALUES ('何泽平',30);Select LAST_INSERT_ID() id";
                var gridReader = await conn.QueryMultipleAsync(databaseUnit, insertSql);
                var id = (await gridReader.ReadAsync<int>()).FirstOrDefault();
                Assert.Equal(1, id);

                using (var grid = await conn.QueryMultipleAsync(databaseUnit, "select 1; select 2; select @x; select 4", new { x = 3 }))
                {
                    var a = await grid.ReadAsync<int>();
                    var b = await grid.ReadAsync<int>();
                    var c = await grid.ReadAsync<int>();
                    var d = await grid.ReadAsync<int>();

                    Assert.Equal(1, a.Single());
                    Assert.Equal(2, b.Single());
                    Assert.Equal(3, c.Single());
                    Assert.Equal(4, d.Single());
                }

            }


        }
        /// <summary>
        /// 测试类型转换函数-枚举
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact]
        public void TestChangeTypeToOtherType_Enum()
        {
            var func = DatabaseContext.ChangeTypeToOtherType<IlEnum>(typeof(int));
            var r1 = func(1);
            Assert.Equal(IlEnum.a, r1);
            var r2 = func(2);
            Assert.Equal(IlEnum.b, r2);

        }

        /// <summary>
        /// 测试类型转换函数-可空类型
        /// </summary>
        [Fact]
        public void TestChangeTypeToOtherType_Nullable()
        {
            //测试可空版本
            var func2 = DatabaseContext.ChangeTypeToOtherType<IlEnum?>(typeof(int));
            var r3 = func2(1);
            Assert.Equal(IlEnum.a, r3);
            var r4 = func2(2);
            Assert.Equal(IlEnum.b, r4);

            var func3 = DatabaseContext.ChangeTypeToOtherType<IlEnum?>(typeof(int?));
            var r5 = func3(1);
            Assert.Equal(IlEnum.a, r5);
            var r6 = func3(2);
            Assert.Equal(IlEnum.b, r6);
       
        }

        /// <summary>
        /// 测试类型转换函数-字符串类型
        /// </summary>
        [Fact]
        public void TestChangeTypeToOtherType_String()
        {
            //测试string 类型
            var func4 = DatabaseContext.ChangeTypeToOtherType<IlEnum?>(typeof(string));
            var r7 = func4("1");
            Assert.Equal(IlEnum.a, r7);
            var r8 = func4("2");
            Assert.Equal(IlEnum.b, r8);
        }

        /// <summary>
        /// 测试类型转换函数-可空类型
        /// </summary>
        [Fact]
        public void TestChangeTypeToOtherType_Dynamic()
        {
            dynamic a = "abc";
            //测试string 类型
            var func4 = DatabaseContext.ChangeTypeToOtherType<dynamic>(typeof(string));
            var r7 = func4("1");
            Assert.Equal("1", r7);
          
        }

        /// <summary>
        /// 测试TypeHandlerCache的SetHandler方法
        /// </summary>
        [Fact, Priority(1010)]
        public void TestGenerateTypeHandlerCacheClass_Parse()
        {
            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }
            var intTypeHandler = new IntTypeHandler();
            var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });
            cacheType.GetMethod("Parse").Invoke(null, new object[] { 123 });


        }

        /// <summary>
        /// 测试TypeHandlerCache的SetHandler方法
        /// </summary>
        [Fact]
        public void TestGenerateTypeHandlerCacheClass_SetHandler()
        {
            var intTypeHandler = new IntTypeHandler();
            var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });
            var result = cacheType.GetField("handler").GetValue(null);
            Assert.Equal(intTypeHandler, result);
        }

        /// <summary>
        /// 测试TypeHandlerCache的SetHandler方法
        /// </summary>
        [Fact, Priority(1003)]
        public void TestGenerateTypeHandlerCacheClass_SetValue()
        {
            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }
            var intTypeHandler = new IntTypeHandler();
            var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                var parameter = cmd.CreateParameter();
                cacheType.GetMethod("SetValue").Invoke(null, new object[] { parameter, 123 });
                Assert.Equal(parameter.Value, 123);
            }


        }

        /// <summary>
        /// 测试返回值为类这种情况
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1004)]
        public void TestQueryWithClass()
        {
            var c = 1 & 2;
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
            databaseUnit.SetTypeHandler(typeof(int), new IntTypeHandler());
            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE test";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE test (
                id varchar(100),
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',30,'" + Guid.NewGuid().ToString() + "')";
                var effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var guidValue = Guid.NewGuid().ToString();
                insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',2,'" + guidValue + "')";
                effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);
                
                var sql = "select * from test where id=@id";
                var result = conn.Query<IlPerson>(databaseUnit, sql, new { id = guidValue }).ToList();
                Assert.Equal(1, result.Count);
                Assert.Equal(new Guid(guidValue), result[0].Id);

                var sql2 = "select age from test where id=@id";
                var result2 = conn.Query<IlEnum?>(databaseUnit, sql2, new { id = guidValue }).ToList();
                Assert.Equal(1, result2.Count);
                Assert.Equal(IlEnum.b, result2[0]);

                var sql3 = "select age from test where age=@age";
                var result3 = conn.Query<IlPerson>(databaseUnit, sql3, new { age = 30 }).ToList();
                Assert.Equal(1, result3.Count);
                Assert.Equal(30, result3[0].Age);

                var sql4 = "select age from test where id=@id";
                var result4 = conn.Query<int>(databaseUnit, sql4, new { id = guidValue }).ToList();
                Assert.Equal(1, result4.Count);
                Assert.Equal(2, result4[0]);
            }
        }

        /// <summary>
        /// 查询单个或默认
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1005)]
        public void TestQueryFirstOrDefaultWithClass()
        {
            var c = 1 & 2;
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
            databaseUnit.SetTypeHandler(typeof(int), new IntTypeHandler());
            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE test";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE test (
                id varchar(100),
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',30,'" + Guid.NewGuid().ToString() + "')";
                var effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var guidValue = Guid.NewGuid().ToString();
                insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',2,'" + guidValue + "')";
                effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var sql = "select * from test where id=@id";
                var result = conn.QueryFirstOrDefault<IlPerson>(databaseUnit, sql, new { id = guidValue });
                Assert.Equal(new Guid(guidValue), result.Id);

                var result2 = conn.QueryFirstOrDefault<IlPerson>(databaseUnit, sql, new { id = Guid.NewGuid().ToString() });
                Assert.Equal(null, result2);
            }
        }

        /// <summary>
        /// 异步查询单个或默认
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1006)]
        public async Task TestQueryFirstOrDefaultWithClassAsync()
        {
            var c = 1 & 2;
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
            databaseUnit.SetTypeHandler(typeof(int), new IntTypeHandler());
            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE test";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE test (
                id varchar(100),
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',30,'" + Guid.NewGuid().ToString() + "')";
                var effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var guidValue = Guid.NewGuid().ToString();
                insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',2,'" + guidValue + "')";
                effectiveRows = conn.Execute(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var sql = "select * from test where id=@id";
                var result =await conn.QueryFirstOrDefaultAsync<IlPerson>(databaseUnit, sql, new { id = guidValue });
                Assert.Equal(new Guid(guidValue), result.Id);

                var result2 = await conn.QueryFirstOrDefaultAsync<IlPerson>(databaseUnit, sql, new { id = Guid.NewGuid().ToString() });
                Assert.Equal(null, result2);
            }
        }

        /// <summary>
        /// 测试异步返回值为类这种情况
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [Fact, Priority(1007)]
        public async Task TestQueryWithClassAsync()
        {
            //var intTypeHandler = new IntTypeHandler();
            //var cacheType = DatabaseContext.GenerateTypeHandlerCacheClass(typeof(int));
            //cacheType.GetMethod("SetHandler").Invoke(null, new object[] { intTypeHandler });

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
            databaseUnit.SetTypeHandler(typeof(int), new IntTypeHandler());
            databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE test";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE test (
                id varchar(100),
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                var insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',30,'" + Guid.NewGuid().ToString() + "')";
                var effectiveRows = await conn.ExecuteAsync(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var guidValue = Guid.NewGuid().ToString();
                insertSql = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',2,'" + guidValue + "')";
                effectiveRows =await conn.ExecuteAsync(databaseUnit, insertSql);
                Assert.Equal(1, effectiveRows);

                var sql = "select * from test where id=@id";
                var result =(await conn.QueryAsync<IlPerson>(databaseUnit, sql, new { id = guidValue })).ToList();
                Assert.Equal(1, result.Count);
                Assert.Equal(new Guid(guidValue), result[0].Id);

                var sql2 = "select age from test where id=@id";
                var result2 =(await conn.QueryAsync<IlEnum?>(databaseUnit, sql2, new { id = guidValue })).ToList();
                Assert.Equal(1, result2.Count);
                Assert.Equal(IlEnum.b, result2[0]);

                var sql3 = "select age from test where age=@age";
                var result3 = (await conn.QueryAsync<IlPerson>(databaseUnit, sql3, new { age = 30 })).ToList();
                Assert.Equal(1, result3.Count);
                Assert.Equal(30, result3[0].Age);

                var sql4 = "select age from test where id=@id";
                var result4 = (await conn.QueryAsync<int>(databaseUnit, sql4, new { id = guidValue })).ToList();
                Assert.Equal(1, result4.Count);
                Assert.Equal(2, result4[0]);
            }
        }

        /// <summary>
        /// 测试为真则跳转，结论，只有数字0和null，以及false是假，其余都为真，特别强调，-1也为真
        /// 
        /// </summary>
        [Fact]
        public static void TestBrTrue()
        {
            var dynamicMethod = new DynamicMethod("TestBrTrue" + Guid.NewGuid().ToString("N"), typeof(int),
                Type.EmptyTypes);

            var il = dynamicMethod.GetILGenerator();
            var isTrueLabel = il.DefineLabel();
            var isFalseLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldnull);
            //il.Emit(OpCodes.Ldc_I4, 0);
            il.Emit(OpCodes.Brtrue_S, isTrueLabel);
            il.Emit(OpCodes.Ldc_I4, 0);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(isTrueLabel);
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Ret);
            var dd = (Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>));

            var re = dd();
            Assert.Equal(0, re);
        }

        public object TDete(object obj, IlPerson person, ref int tt)
        {
            var c = (int)obj;
            int ccc = 33;
            ref int a = ref ccc;
            return person;
        }


        private class PrivateProperty
        {
            public int Test { get; private set; }
        }
        /// <summary>
        /// 测试属性私有的情况下，是否可以赋值,结论是可以赋值,无论 set是否私有，都是CanWrite,没有set才不是canWrite。
        /// </summary>
        [Fact]
        public static void TestPrivateProperty()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[] { typeof(PrivateProperty) });

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Callvirt, typeof(PrivateProperty).GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(it => it.Name == "Test").SetMethod);
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Ret);

            var dd = (Func<PrivateProperty, int>)dynamicMethod.CreateDelegate(typeof(Func<PrivateProperty, int>));
            var param = new PrivateProperty()
            {

            };
            var re = dd(param);
            Assert.Equal(10, re);
        }

        public delegate object Dete(object obj, IlPerson person, ref int tt);

        /// <summary>
        /// 测试unbox指令，该指令作用为，将堆栈顶部的对象引用，拆箱为一个值类型指针，即ref类型，主要用在结构体赋值中。
        /// </summary>
        [Fact]
        public static void TestUnbox()
        {

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object), new Type[] { typeof(object), typeof(int) });
            var ageProp = typeof(IlValueTypeItem).GetProperty(nameof(IlValueTypeItem.Age)).GetSetMethod();
            var il = dynamicMethod.GetILGenerator();
            var local = il.DeclareLocal(typeof(IlValueTypeItem).MakeByRefType());

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Unbox, typeof(IlValueTypeItem));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, ageProp);

            il.Emit(OpCodes.Ret);
            var dd = (Func<object, int, object>)dynamicMethod.CreateDelegate(typeof(Func<object, int, object>));
            var valueItem = new IlValueTypeItem()
            {
                Age = 30,
                Name = "ppx"
            };
            var re = dd(valueItem, 10);
            Assert.Equal(10, ((IlValueTypeItem)re).Age);
        }

        /// <summary>
        /// 测试ref传参，使用stind指令来设置指针，先推送地址到堆栈，再推送值，然后调用stind指令，会把值赋给相应的地址，堆栈清空
        /// </summary>
        [Fact]
        public static void TestRefAndStind()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object), new Type[] { typeof(object), typeof(IlPerson), typeof(int).MakeByRefType() });
            // var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),Type.EmptyTypes);
            var ageProp = typeof(IlPerson).GetProperty(nameof(IlPerson.Age)).GetSetMethod();
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Stind_I4);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            //il.Emit(OpCodes.Starg_S,2);
            //il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ageProp);
            il.Emit(OpCodes.Ret);
            //var refType= Expression.GetFuncType(typeof(object), typeof(int).MakeByRefType());
            //var dd = dynamicMethod.CreateDelegate(refType);
            //var dd = (Func<object, IlPerson, object>) dynamicMethod.CreateDelegate(typeof(Func<object, IlPerson, object>));
            var dd = (Dete)dynamicMethod.CreateDelegate(typeof(Dete));

            var ilPerson = new IlPerson()
            {
                Age = 3,
                Name = "何泽平"
            };
            int intValue = 6;
            var re = dd(10, ilPerson, ref intValue);
            //var c2 = re.GetType();
            Assert.Equal(10, intValue);
        }

        [Fact]
        public static void TestMatch()
        {
            int intV2 = 10;
            ref int intV = ref intV2;
            intV = 20;

            var a = new Regex(@"(?<![\p{L}\p{N}_])\{=([\p{L}\p{N}_]+)\}",
                 RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant |
                 RegexOptions.Compiled);
            var sql = "select a.age,a.name from a where a.name=@ppx";
            var c = a.Matches(sql);
        }


        /// <summary>
        /// 测试引用类型传递
        /// </summary>
        [Fact]
        public static void TestReferenceModelUpdate()
        {
            var person = new IlPerson()
            {
                Name = "何泽平",
                Age = 30
            };
            var person2 = person;
            person = new IlPerson()
            {
                Name = "xxx"
            };
            UpdateIlPerson(person);
            Assert.Equal("ppx", person.Name);
            Assert.Equal("何泽平", person2.Name);
        }

        public static void UpdateIlPerson(IlPerson person)
        {
            person.Name = "ppx";
            person = new IlPerson()
            {
                Name = "hhh",
                Age = 1
            };
        }

        /// <summary>
        /// 测试try catch
        /// </summary>
        [Fact]
        public static void TestTryCatch2()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[] { typeof(string) });
            ILGenerator methodIL = dynamicMethod.GetILGenerator();
            LocalBuilder num = methodIL.DeclareLocal(typeof(Int32));

            //int num = 0;
            methodIL.Emit(OpCodes.Ldc_I4_0);
            methodIL.Emit(OpCodes.Stloc_0);

            //begin try
            Label tryLabel = methodIL.BeginExceptionBlock();
            //num = Convert.ToInt32(str);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(string) }));
            methodIL.Emit(OpCodes.Stloc_0);
            //end try

            //begin catch 注意，这个时侯堆栈顶为异常信息ex
            methodIL.BeginCatchBlock(typeof(Exception));
            //Console.WriteLine(ex.Message);
            methodIL.Emit(OpCodes.Call, typeof(Exception).GetMethod("get_Message"));
            methodIL.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            //end catch
            methodIL.EndExceptionBlock();

            //return num;
            methodIL.Emit(OpCodes.Ldloc_0);
            methodIL.Emit(OpCodes.Ret);
            var dd = (Func<string, int>)dynamicMethod.CreateDelegate(typeof(Func<string, int>));
            var re = dd("123");
            //Assert.Equal("何泽平", re);
        }
        /// <summary>
        /// 测试try catch,经过测试，进入try，catch之前，堆栈必须为空，try catch结束后，trycatch里的堆栈也会被清空
        /// </summary>
        [Fact]
        public static void TestTryCatch()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(string),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var strLocal = il.DeclareLocal(typeof(string));
            //il.Emit(OpCodes.Ldstr, "何泽平");
            //il.Emit(OpCodes.Pop);
            var tryLabel = il.BeginExceptionBlock();
            il.Emit(OpCodes.Ldstr, "ab0");
            il.Emit(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ToInt32), new Type[] { typeof(string) }));
            il.Emit(OpCodes.Pop);
            //il.Emit(OpCodes.Leave_S, tryLabel);
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Call, typeof(IlGeneratorTest).GetMethod(nameof(IlGeneratorTest.PrintException)));
            il.Emit(OpCodes.Ldstr, "异常里挑出");
            il.SteadOfLocal(strLocal);
            //il.Emit(OpCodes.Leave_S, tryLabel);
            //il.Emit(OpCodes.Call, typeof(Exception).GetMethod("get_Message"));
            //il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            ////il.Emit(OpCodes.Pop);
            ////il.Emit(OpCodes.Ret);
            //il.Emit(OpCodes.Ldstr, "异常里挑出");
            //il.Emit(OpCodes.Ret);
            il.EndExceptionBlock();
            //il.Emit(OpCodes.Call, typeof(IlGeneratorTest).GetMethod(nameof(IlGeneratorTest.PrintObject)));
            il.Emit(OpCodes.Ldloc, strLocal);
            il.Emit(OpCodes.Ret);
            var dd = (Func<string>)dynamicMethod.CreateDelegate(typeof(Func<string>));
            var re = dd();
            Assert.Equal("异常里挑出", re);
        }
        /// <summary>
        /// 测试try catch,毕竟用自定义的错误处理程序
        /// </summary>
        [Fact]
        public static void TestTryCatchWithCustomExceptionHandler()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), null,
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            //il.Emit(OpCodes.Ldstr, "何泽平");
            il.BeginExceptionBlock();
            il.Emit(OpCodes.Ldstr, "abc");
            il.Emit(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ToInt32), new Type[] { typeof(string) }));
            il.Emit(OpCodes.Pop);
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Call, typeof(IlGeneratorTest).GetMethod(nameof(IlGeneratorTest.PrintException)));
            //il.Emit(OpCodes.Call, typeof(Exception).GetMethod("get_Message"));
            //il.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new Type[] { typeof(string) }));

            ////il.Emit(OpCodes.Ret);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
            var dd = (Action)dynamicMethod.CreateDelegate(typeof(Action));
            dd();

        }

        /// <summary>
        /// 测试可空类型转为非可空类型
        /// </summary>
        [Fact]
        public static void TestNullableTypeToType()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int?), Type.EmptyTypes);
            // var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),Type.EmptyTypes);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Newobj, typeof(int?).GetConstructor(new[] { typeof(int) }));
            il.Emit(OpCodes.Box, typeof(int?));
            il.Emit(OpCodes.Unbox_Any, typeof(int?));
            il.Emit(OpCodes.Ret);
            var dd = (Func<int?>)dynamicMethod.CreateDelegate(typeof(Func<int?>));
            // var dd = (Func<object>) dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = dd();
            Assert.Equal(10, re);
        }

        public static void Test3()
        {
            int? a = 3;
            object b = a;
            int? c = (int?)b;
        }

        /// <summary>
        /// 测试值类型赋值为可空类型,方式1，装箱再拆箱,装箱拆箱自带了一个转换效果
        /// </summary>
        [Fact]
        public static void TestTypeToNullableTypeUseBoxAndUnbox()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int?),
                Type.EmptyTypes);
            var ctor = typeof(int?).GetConstructor(new Type[1] { typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var objLocal = il.DeclareLocal(typeof(int?));

            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Unbox_Any, typeof(int?));

            il.Emit(OpCodes.Ret);

            var dd = (Func<int?>)dynamicMethod.CreateDelegate(typeof(Func<int?>));
            var re = dd();
            Assert.Equal(10, re);
        }

        /// <summary>
        /// 测试值类型赋值为可空类型,方式2，newobj指令，通过构造函数生成一个可空类型,
        /// </summary>
        [Fact]
        public static void TestTypeToNullableTypeUseNewobj()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int?),
                Type.EmptyTypes);
            var ctor = typeof(int?).GetConstructor(new Type[1] { typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var objLocal = il.DeclareLocal(typeof(int?));
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            var dd = (Func<int?>)dynamicMethod.CreateDelegate(typeof(Func<int?>));
            var re = dd();
            Assert.Equal(10, re);
        }

        public static void PrintObject(object obj)
        {

        }
        public static void PrintException(Exception ex)
        {

        }
        /// <summary>
        /// 测试栈顶有很多数据是否可以正确返回,有pop弹出则正常，否则报错，说明方法结束时，堆栈里必须为空
        /// </summary>
        [Fact]
        public static void TestTopOfStackHasManyDataThenReturn()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(string),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "何泽平");
            il.Emit(OpCodes.Ldstr, "0");
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            var dd = (Func<string>)dynamicMethod.CreateDelegate(typeof(Func<string>));
            var re = dd();
            Assert.Equal("何泽平", re);
        }

        /// <summary>
        /// 测试可空值类型是否为值类型,结论为真，是值类型
        /// </summary>
        [Fact]
        public static void TestNullableValueType()
        {
            Assert.Equal(true, typeof(int?).IsValueType);
        }

        /// <summary>
        /// 测试反射获取属性
        /// </summary>
        [Fact, Priority(1008)]
        public static void TestReader()
        {

            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "DROP TABLE test";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }

                cmd.CommandText = @"CREATE TABLE test (
                id varchar(100),
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"INSERT INTO test (Name,Age,id) VALUES (null,30,'" + Guid.NewGuid().ToString("N") + "')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"INSERT INTO test (Name,Age,id) VALUES ('何泽平',30,'" + Guid.NewGuid().ToString("N") + "')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "select * from test";
                IDataReader dr = cmd.ExecuteReader();
                var list = new List<IlPerson>();


                var databaseUnit = new DatabaseUnit(typeof(IUnitOfWork), typeof(MySqlConnection), connectionString);
                databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

                while (dr.Read())
                {
                    //var c = dr.GetFieldType(0);
                    //var c1 = dr.GetFieldType(1);
                    var func = DatabaseContext.GetTypeDeserializer(typeof(IlPerson), dr, databaseUnit);
                    var result = func(dr);
                    list.Add((IlPerson)result);
                }

                conn.Close();
            }

            var p1 = typeof(IlResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.Equal(1, p1.Length);
        }


        /// <summary>
        /// 测试可空类型,执行Nullable.GetUnderlyingType，如果是可行类型，返回原生类型，如果非可空类型，返回null,可空类型本身也为值类型
        /// </summary>
        [Fact]
        public static void TestNullableType()
        {
            var p1 = typeof(IlNullable).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.Equal(2, p1.Length);
            var type = p1[0].PropertyType;
            Assert.True(type.IsValueType);
            var nullableType = Nullable.GetUnderlyingType(type);
            Assert.Equal(typeof(int), nullableType);
            var type2 = p1[1].PropertyType;
            var nullableType2 = Nullable.GetUnderlyingType(type2);
            Assert.Null(nullableType2);
            var c = Type.GetTypeCode(type);
            var c2 = Type.GetTypeCode(type2);
        }


        /// <summary>
        /// 测试枚举类型,执行Enum.GetUnderlyingType，获取枚举的实际类型，枚举可以是byte int long类型等
        /// </summary>
        [Fact]
        public static void TestEnumGetUnderlyingType()
        {
            var p1 = typeof(IlEnumBody).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.Equal(1, p1.Length);
            var type = p1[0].PropertyType;
            var nullableType = Enum.GetUnderlyingType(type);
            Assert.Equal(typeof(long), nullableType);
        }

        /// <summary>
        /// 测试string类型是否为类类型
        /// </summary>
        [Fact]
        public static void TestStringIsClass()
        {
            var result = typeof(string).IsClass;
            Assert.True(result);
        }

        /// <summary>
        /// 测试一个Type是否为数字类型
        /// </summary>
        [Fact]
        public static void TestIsNumberType()
        {

            var result = typeof(string).IsNumberType();
            Assert.False(result);
            var decimalResult = typeof(decimal).IsNumberType();
            Assert.True(decimalResult);
        }

        /// <summary>
        /// 测试测试对象引用是否为特定类的实例,bool测试，如果是，返回true，如果否，返回false
        /// </summary>
        [Fact]
        public static void TestIsInstanceWithBool()
        {
            var dynamicMethod = new DynamicMethod("TestIsInstanceWithBool" + Guid.NewGuid().ToString("N"), typeof(bool),
                Type.EmptyTypes);
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var objLocal = il.DeclareLocal(typeof(decimal));

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Isinst, typeof(IlPerson));
            il.Emit(OpCodes.Ret);

            var dd = (Func<bool>)dynamicMethod.CreateDelegate(typeof(Func<bool>));
            var re = dd();
            Assert.True(re);
        }

        [Fact]
        public static void TestIsInstanceWithObject()
        {
            var dynamicMethod = new DynamicMethod("TestIsInstance" + Guid.NewGuid().ToString("N"), typeof(bool),
                Type.EmptyTypes);
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Isinst, typeof(object));
            il.Emit(OpCodes.Ret);

            var dd = (Func<bool>)dynamicMethod.CreateDelegate(typeof(Func<bool>));
            var re = dd();
            Assert.True(re);
        }

        /// <summary>
        /// 测试测试对象引用是否为特定类的实例,如果是，类似于castClass的效果，否则返回空引用
        /// </summary>
        [Fact]
        public static void TestIsInstanceWithClass()
        {
            var nullDynamicMethod = new DynamicMethod("TestIsInstance" + Guid.NewGuid().ToString("N"), typeof(IlPerson),
                Type.EmptyTypes);
            var ctor2 = typeof(DogClass3).GetConstructor(Type.EmptyTypes);
            var il2 = nullDynamicMethod.GetILGenerator();
            il2.Emit(OpCodes.Newobj, ctor2);
            il2.Emit(OpCodes.Isinst, typeof(IlPerson));
            il2.Emit(OpCodes.Ret);

            var nullDd = (Func<IlPerson>)nullDynamicMethod.CreateDelegate(typeof(Func<IlPerson>));
            var re2 = nullDd();
            Assert.Null(re2);

            var dynamicMethod = new DynamicMethod("TestIsInstance" + Guid.NewGuid().ToString("N"), typeof(IlPerson),
                Type.EmptyTypes);
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Isinst, typeof(IlPerson));
            il.Emit(OpCodes.Ret);
           
            var dd = (Func<IlPerson>)dynamicMethod.CreateDelegate(typeof(Func<IlPerson>));
            var re = dd();
            Assert.NotNull(re);
            Assert.Equal(0, re.Age);
        }

        /// <summary>
        /// 测试测试对象引用是否为特定类的实例。
        /// </summary>
        [Fact]
        public static void TestIsInstanceObjectType()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),
                new Type[] { typeof(object) });
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.Box, typeof(long));

            il.Emit(OpCodes.Callvirt, typeof(object).GetMethod(nameof(object.GetType)));
            //il.Emit(OpCodes.Box,typeof(long));
            ////il.Emit(OpCodes.Box, typeof(long));
            //il.Emit(OpCodes.Box, typeof(object));
            //il.Emit(OpCodes.Unbox_Any, typeof(IlEnum));
            //il.Emit(OpCodes.Box, typeof(object));
            il.Emit(OpCodes.Ret);

            var dd = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            var re = dd(2L);
            //Assert.True(re);
            ////Assert.Equal(IlEnum.b,(IlEnum)re);
        }

        /// <summary>
        /// 测试ldToken指令，结论，这指令将编一阶段的元数据令牌转换为运行时的数据type等
        /// </summary>
        [Fact]
        public static void TestLdToken()
        {

            //var re2=  method.Invoke(null,new Object[]{1L});
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(Type),
                new Type[] { typeof(object) });
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldtoken, typeof(IlPerson));

            il.Emit(OpCodes.Ret);

            var dd = (Func<Type>)dynamicMethod.CreateDelegate(typeof(Func<Type>));
            var re = dd();
            ;
            Assert.Equal(typeof(IlPerson), re);
        }

        [Fact]
        public static void TestLdTokenWithTypeHandle()
        {

            //var re2=  method.Invoke(null,new Object[]{1L});
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(Type),
                new Type[] { typeof(object) });
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldtoken, typeof(IlPerson));
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
            il.Emit(OpCodes.Ret);

            var dd = (Func<Type>)dynamicMethod.CreateDelegate(typeof(Func<Type>));
            var re = dd();
            ;
            Assert.Equal(typeof(IlPerson), re);
        }

        [Fact]

        public static void TestGenerateTypeMethod()
        {
            var method = typeof(IlGeneratorTest).GetMethod(nameof(IlGeneratorTest.GenerateTypeMethod));
            //var re2= method.Invoke(null,new Object[]{1L,typeof(IlPerson)});
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(bool),
                new Type[] { typeof(object) });
            var ctor = typeof(IlPerson).GetConstructor(Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Box, typeof(int));

            il.Emit(OpCodes.Ldtoken, typeof(int));
            il.Emit(OpCodes.Call, SbUtil.GetTypeFromHandleMethod);
            il.Emit(OpCodes.Call, method);

            il.Emit(OpCodes.Ret);

            var dd = (Func<bool>)dynamicMethod.CreateDelegate(typeof(Func<bool>));
            var re = dd();
            Assert.True(re);
        }

        public static bool GenerateTypeMethod(object value, Type type)
        {
            //var a = typeof(T);
            var b = value?.GetType() == type;
            return true;
        }

        [Fact]
        public static void TestSizeOf()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                Type.EmptyTypes);

            var il = dynamicMethod.GetILGenerator();
            //il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Sizeof, typeof(short));

            il.Emit(OpCodes.Ret);

            var dd = (Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>));
            var re = dd();
            //Assert.True(re);
            Assert.Equal(2, re);
        }

        [Fact]
        public static void T5()
        {
            decimal f = 1m;
            object d = f;

            var g = d.GetType();
            var f2 = d is decimal;

        }

        /// <summary>
        /// 测试op_Explicit显示类型转换,这里以decimal显示转换为double为例。
        /// </summary>
        [Fact]
        public static void TestOp_Explicit()
        {
            var op_method = typeof(decimal).GetMethods().Where(it => it.Name == "op_Explicit" && it.ReturnType == typeof(double) && it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == typeof(decimal)).ToList().First();
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(double),
                Type.EmptyTypes);

            var ctor = typeof(decimal).GetConstructor(new Type[1] { typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var objLocal = il.DeclareLocal(typeof(decimal));
            //il.Emit(OpCodes.Ldloca_S,objLocal);
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Call, op_method);
            il.Emit(OpCodes.Ret);

            var dd = (Func<double>)dynamicMethod.CreateDelegate(typeof(Func<double>));
            var re = dd();
            Assert.Equal(10, re);
        }

        [Fact]
        public static void TestConvertChangeType()
        {
            var c2 = typeof(DateTime);
            var c = Convert.ChangeType(2L, typeof(string));
            var op_method = typeof(decimal).GetMethods().Where(it => it.Name == "op_Explicit" && it.ReturnType == typeof(double) && it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == typeof(decimal)).ToList().First();
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(double),
                Type.EmptyTypes);

            var ctor = typeof(decimal).GetConstructor(new Type[1] { typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var objLocal = il.DeclareLocal(typeof(decimal));
            //il.Emit(OpCodes.Ldloca_S,objLocal);
            il.Emit(OpCodes.Ldc_I4, 10);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Call, op_method);
            il.Emit(OpCodes.Ret);

            var dd = (Func<double>)dynamicMethod.CreateDelegate(typeof(Func<double>));
            var re = dd();
            Assert.Equal(10, re);
        }

        /// <summary>
        /// 测试CastClass方法，转换类型
        /// </summary>
        [Fact]
        public static void TestCastClass()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(IlPerson),
                new Type[] { typeof(IlPersonSubClass) });
            var il = dynamicMethod.GetILGenerator();
            var lable = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, typeof(IlPerson));
            il.Emit(OpCodes.Ret);

            var dd = (Func<IlPersonSubClass, IlPerson>)dynamicMethod.CreateDelegate(typeof(Func<IlPersonSubClass, IlPerson>));
            var re = dd(new IlPersonSubClass() { Address = "路边", Age = 1, Name = "草药" });
            Assert.Equal("草药", re.Name);
        }

        /// <summary>
        /// 测试可空类型,执行Nullable.GetUnderlyingType，如果是可行类型，返回原生类型，如果非可空类型，返回null
        /// </summary>
        [Fact]
        public static void TestGetTypeCode()
        {
            var p1 = typeof(IlNullable).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.Equal(2, p1.Length);
            var type = p1[0].PropertyType;
            var c = Type.GetTypeCode(type);
            if (c == TypeCode.Int32)
            {

            }
            var nullableType = Nullable.GetUnderlyingType(type);
            Assert.Equal(typeof(int), nullableType);
            var type2 = p1[1].PropertyType;
            var nullableType2 = Nullable.GetUnderlyingType(type2);
            Assert.Null(nullableType2);
        }

        /// <summary>
        /// 测试反射获取属性
        /// </summary>
        [Fact]
        public static void TestGetProperty()
        {
            var p1 = typeof(IlResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.Equal(1, p1.Length);

        }

        /// <summary>
        /// 测试通过反射获取getItem方法
        /// </summary>
        [Fact]
        public static void TestGetItemMethod()
        {
            var p1 = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(it => it.Name == "get_Item" && it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == typeof(int));
            //;
            Assert.NotNull(p1);
        }

        /// <summary>
        /// 测试long类型，即int64，特别要注意，入参时如果输入数字，默认为int32，需要强制转换为long。
        /// </summary>
        [Fact]
        public static void TestLong()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(long),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var lable = il.DeclareLocal(typeof(object));
            var ctor = typeof(IlEnumBody).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Ldc_I8, (long)1);
            il.Emit(OpCodes.Ret);
            var dd = (Func<long>)dynamicMethod.CreateDelegate(typeof(Func<long>));
            var re = dd();
            Assert.Equal(1, re);
        }

        /// <summary>
        /// 测试其他类型到enum的转换
        /// </summary>
        [Fact]
        public static void TestEnumParse()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(IlEnum),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var lable = il.DeclareLocal(typeof(object));
            var ctor = typeof(IlEnumBody).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Ldc_I4, 1);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Box, typeof(long));
            il.Emit(OpCodes.Unbox_Any, typeof(IlEnum));
            il.Emit(OpCodes.Ret);
            var dd = (Func<IlEnum>)dynamicMethod.CreateDelegate(typeof(Func<IlEnum>));
            var re = dd();
            //Assert.Equal(IlEnum.a, re);
        }

        /// <summary>
        /// 测试其他类型到enum的转换,并且设置为类的属性
        /// </summary>
        [Fact]
        public static void TestEnumParseAndSettingProperty()
        {
            //var c=  Convert.ChangeType(1m, typeof(bool));
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(IlEnumBody),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var ctor = typeof(IlEnumBody).GetConstructor(Type.EmptyTypes);

            il.Emit(OpCodes.Newobj, ctor); //stack is ilResult
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, 2);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Box, typeof(long));
            il.Emit(OpCodes.Unbox_Any, typeof(IlEnum));
            il.Emit(OpCodes.Call, typeof(IlEnumBody).GetProperty(nameof(IlEnumBody.Enum)).GetSetMethod());
            il.Emit(OpCodes.Ret);

            var dd = (Func<IlEnumBody>)dynamicMethod.CreateDelegate(typeof(Func<IlEnumBody>));
            var re = dd();
            Assert.Equal(IlEnum.b, re.Enum);
        }

        [Fact]
        public static void Test()
        {
            var op_method = typeof(decimal).GetMethods().Where(it => it.Name == "op_Explicit" && it.ReturnType == typeof(uint)).ToList();
            var c = typeof(int).IsPrimitive;
            var d = typeof(string).IsPrimitive;
        }

        /// <summary>
        /// 测试Console.WriteLine方法，方便打印输出
        /// </summary>
        [Fact]
        public static void TestConsoleWriteLine()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var lable = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldstr, "测试");
            il.Emit(OpCodes.Call, typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(object) }));
            il.Emit(OpCodes.Ldstr, "测试");
            il.Emit(OpCodes.Ret);

            var dd = (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = dd();
            Assert.Equal("测试", re);
        }

        /// <summary>
        /// 测试InitObject和Ldloca，该指令仅针对值类型的结构体，而不是原生的int等
        /// </summary>
        [Fact]
        public static void TestInitObjectAndLdloca()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var ilRe = il.DeclareLocal(typeof(IlValueTypeItem));
            var nameProperty = typeof(IlValueTypeItem).GetProperty("Name");
            var ctor = typeof(IlResult).GetConstructor(Type.EmptyTypes);

            il.Emit(OpCodes.Ldloca, ilRe);

            il.Emit(OpCodes.Initobj, typeof(IlValueTypeItem)); //stack is 

            il.Emit(OpCodes.Ldloca, ilRe);
            il.Emit(OpCodes.Ldstr, "何泽平");
            il.Emit(OpCodes.Call, nameProperty.GetSetMethod());
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box, typeof(IlValueTypeItem));
            il.Emit(OpCodes.Ret);

            var dd = (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = (IlValueTypeItem)dd();
            Assert.Equal("何泽平", re.Name);
        }

        /// <summary>
        /// 测试il装箱操作,box指令后面跟着值类型的type类型
        /// </summary>
        [Fact]
        public static void TestBox()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var ctor = typeof(IlResult).GetConstructor(Type.EmptyTypes);
            // il.Emit(OpCodes.Initobj, typeof(int)); //stack is 
            // il.Emit(OpCodes.Newobj,ctor);
            il.Emit(OpCodes.Ldc_I4_1);

            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Ret);
            var cccc = typeof(IlResult).GetConstructors();

            var dd = (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = dd();
            Assert.Equal(1, re);
        }

        public object test()
        {
            object c = 1;
            return c;
        }

        /// <summary>
        /// 测试方法里返回一个实体类
        /// </summary>
        [Fact]
        public static void TestReturnNewClass()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(object),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            var ctor = typeof(IlResult).GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, ctor); //stack is ilResult
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldstr, "何泽平");
            il.Emit(OpCodes.Call, typeof(IlResult).GetProperty("Name").GetSetMethod());
            il.Emit(OpCodes.Ret);

            var cccc = typeof(IlResult).GetConstructors();
            //var dd = (Func<Type, IlResult>)dynamicMethod.CreateDelegate(typeof(Func<Type, IlResult>));
            var dd = (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = (IlResult)dd();
            Assert.Equal("何泽平", re.Name);
        }


        /// <summary>
        /// 测试常规加减法
        /// </summary>
        [Fact]
        public static void TestAdd()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[] { typeof(int), typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var ret = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Add);
            il.SteadOfLocal(ret);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            var dd = (Func<int, int, int>)dynamicMethod.CreateDelegate(typeof(Func<int, int, int>));
            var re = dd(5, 6);
            Assert.Equal(11, re);
        }

        /// <summary>
        /// if 和else语句
        /// </summary>
        [Fact]
        public static void TestIfElse()
        {
            var dynamicMethod = new DynamicMethod("test2" + Guid.NewGuid().ToString("N"), typeof(string),
                new Type[] { typeof(int) });
            var debugWriteMethod = typeof(Debug)
                .GetMethod(nameof(Debug.WriteLine), new Type[] { typeof(object) });
            var il = dynamicMethod.GetILGenerator();
            var endLabel = il.DefineLabel();
            var elseLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, 5);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brfalse_S, elseLabel);
            il.Emit(OpCodes.Ldstr, "相等");
            il.Emit(OpCodes.Br_S, endLabel);
            il.MarkLabel(elseLabel);
            il.Emit(OpCodes.Ldstr, "不相等");
            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
            var func = (Func<int, string>)dynamicMethod.CreateDelegate(typeof(Func<int, string>));
            var re = func(5);
            Assert.Equal("相等", re);
            var re2 = func(6);
            Assert.Equal("不相等", re2);
        }

        /// <summary>
        /// 测试跳转标签，结果是标签可以定义了不使用，并不会报错
        /// </summary>
        [Fact]
        public static void TestLabel()
        {
            var dynamicMethod = new DynamicMethod("test2" + Guid.NewGuid().ToString("N"), typeof(int),
                Type.EmptyTypes);
            var debugWriteMethod = typeof(Debug)
                .GetMethod(nameof(Debug.WriteLine), new Type[] { typeof(object) });
            var il = dynamicMethod.GetILGenerator();
            var endLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldc_I4, 10);
            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
            var func = (Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>));
            var re = func();
            Assert.Equal(10, re);
        }

        /// <summary>
        /// 测试无条件跳转标签是否会消耗栈顶，结论Brtrue_S会消耗栈顶元素并跳转，Leave_S指令会清空栈顶，同时markLabel相当于分支，每种分支都要写
        ///
        /// /// </summary>
        [Fact]
        public static void TestBrtrue_SLabel()
        {
            var dynamicMethod = new DynamicMethod("test2" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[] { typeof(int) });
            var debugWriteMethod = typeof(Debug)
                .GetMethod(nameof(Debug.WriteLine), new Type[] { typeof(object) });
            var il = dynamicMethod.GetILGenerator();
            var endLabel = il.DefineLabel();
            var finishLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Brtrue_S, endLabel);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Leave_S, finishLabel);
            il.MarkLabel(endLabel);
            //il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(finishLabel);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
            var func = (Func<int, int>)dynamicMethod.CreateDelegate(typeof(Func<int, int>));
            var re = func(1);
            Assert.Equal(1, re);
            var re2 = func(0);
            Assert.Equal(0, re2);
        }

        /// <summary>
        /// 测试类型转换，数值到string
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNumberToString()
        {
            //数值转string

            //bool =>string
            Delegate dd; object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(string));
            re = dd.DynamicInvoke(true);
            Assert.Equal("True", re);

            //byte=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte), typeof(string));
            re = dd.DynamicInvoke((byte)1);
            Assert.Equal("1", re);
            //sbyte=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte), typeof(string));
            re = dd.DynamicInvoke((sbyte)1);
            Assert.Equal("1", re);
            //short=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short), typeof(string));
            re = dd.DynamicInvoke((short)1);
            Assert.Equal("1", re);
            //ushort=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort), typeof(string));
            re = dd.DynamicInvoke((ushort)1);
            Assert.Equal("1", re);
            //int=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int), typeof(string));
            re = dd.DynamicInvoke((int)1);
            Assert.Equal("1", re);
            //uint=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint), typeof(string));
            re = dd.DynamicInvoke((uint)1);
            Assert.Equal("1", re);
            //long=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long), typeof(string));
            re = dd.DynamicInvoke((long)1);
            Assert.Equal("1", re);
            //ulong=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong), typeof(string));
            re = dd.DynamicInvoke((ulong)1);
            Assert.Equal("1", re);

            //float=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float), typeof(string));
            re = dd.DynamicInvoke((float)1);
            Assert.Equal("1", re);
            //double=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double), typeof(string));
            re = dd.DynamicInvoke((double)1);
            Assert.Equal("1", re);
            //decimal=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(string));
            re = dd.DynamicInvoke((decimal)1);
            Assert.Equal("1", re);

            //开始反转
            //string =>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(bool));
            re = dd.DynamicInvoke("true");
            Assert.Equal(true, re);
            //string =>byte
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(byte));
            re = dd.DynamicInvoke("1");
            Assert.Equal((byte)1, re);

            //string =>sbyte
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(sbyte));
            re = dd.DynamicInvoke("1");
            Assert.Equal((sbyte)1, re);

            //string =>short
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(short));
            re = dd.DynamicInvoke("1");
            Assert.Equal((short)1, re);

            //string =>ushort
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(ushort));
            re = dd.DynamicInvoke("1");
            Assert.Equal((ushort)1, re);

            //string =>uint
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(uint));
            re = dd.DynamicInvoke("1");
            Assert.Equal((uint)1, re);

            //string =>int
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(int));
            re = dd.DynamicInvoke("1");
            Assert.Equal((int)1, re);

            //string =>long
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(long));
            re = dd.DynamicInvoke("1");
            Assert.Equal((long)1, re);

            //string =>ulong
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(ulong));
            re = dd.DynamicInvoke("1");
            Assert.Equal((ulong)1, re);

            //string =>float
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(float));
            re = dd.DynamicInvoke("1");
            Assert.Equal((float)1, re);

            //string =>double
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(double));
            re = dd.DynamicInvoke("1");
            Assert.Equal((double)1, re);

            //string =>decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(decimal));
            re = dd.DynamicInvoke("1");
            Assert.Equal((decimal)1, re);
        }

        /// <summary>
        /// 测试类型转换，可空数值到string
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNullableNumberToString()
        {
            //数值转string

            //bool? =>string
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(string));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal("True", re);

            //byte?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte?), typeof(string));
            re = dd.DynamicInvoke((byte?)1);
            Assert.Equal("1", re);
            //sbyte?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte?), typeof(string));
            re = dd.DynamicInvoke((sbyte?)1);
            Assert.Equal("1", re);
            //short?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short?), typeof(string));
            re = dd.DynamicInvoke((short?)1);
            Assert.Equal("1", re);
            //ushort?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort?), typeof(string));
            re = dd.DynamicInvoke((ushort?)1);
            Assert.Equal("1", re);
            //int?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int?), typeof(string));
            re = dd.DynamicInvoke((int?)1);
            Assert.Equal("1", re);
            //uint?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint?), typeof(string));
            re = dd.DynamicInvoke((uint?)1);
            Assert.Equal("1", re);
            //long?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long?), typeof(string));
            re = dd.DynamicInvoke((long?)1);
            Assert.Equal("1", re);
            //ulong?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong?), typeof(string));
            re = dd.DynamicInvoke((ulong?)1);
            Assert.Equal("1", re);

            //float?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float?), typeof(string));
            re = dd.DynamicInvoke((float?)1);
            Assert.Equal("1", re);
            //double?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double?), typeof(string));
            re = dd.DynamicInvoke((double?)1);
            Assert.Equal("1", re);
            //decimal?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(string));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal("1", re);

            //开始反转
            //string =>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(bool?));
            re = dd.DynamicInvoke("true");
            Assert.Equal(true, re);
            //string =>byte?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(byte?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((byte?)1, re);

            //string =>sbyte?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(sbyte?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((sbyte?)1, re);

            //string =>short?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(short?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((short?)1, re);

            //string =>ushort?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(ushort?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((ushort?)1, re);

            //string =>uint?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(uint?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((uint?)1, re);

            //string =>int?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(int?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((int?)1, re);

            //string =>long?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(long?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((long?)1, re);

            //string =>ulong?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(ulong?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((ulong?)1, re);

            //string =>float?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(float?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((float?)1, re);

            //string =>double?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(double?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((double?)1, re);

            //string =>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(decimal?));
            re = dd.DynamicInvoke("1");
            Assert.Equal((decimal?)1, re);
        }

        /// <summary>
        /// 测试类型转换，数值转数值
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNumberToNumber()
        {
            //数值转数值

            //bool =>byte
            Delegate dd; object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(byte));
            re = dd.DynamicInvoke(true);
            Assert.Equal((byte)1, re);

            //bool =>sbyte
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(sbyte));
            re = dd.DynamicInvoke(true);
            Assert.Equal((sbyte)1, re);

            //bool =>short
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(short));
            re = dd.DynamicInvoke(true);
            Assert.Equal((short)1, re);

            //bool =>ushort
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(ushort));
            re = dd.DynamicInvoke(true);
            Assert.Equal((ushort)1, re);

            //bool =>uint
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(uint));
            re = dd.DynamicInvoke(true);
            Assert.Equal((uint)1, re);

            //bool =>int
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(int));
            re = dd.DynamicInvoke(true);
            Assert.Equal((int)1, re);

            //bool =>long
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(long));
            re = dd.DynamicInvoke(true);
            Assert.Equal((long)1, re);

            //bool =>ulong
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(ulong));
            re = dd.DynamicInvoke(true);
            Assert.Equal((ulong)1, re);

            //bool =>float
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(float));
            re = dd.DynamicInvoke(true);
            Assert.Equal((float)1, re);

            //bool =>double
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(double));
            re = dd.DynamicInvoke(true);
            Assert.Equal((double)1, re);
            //开始反转
            //bool=>bool 

            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(bool));
            re = dd.DynamicInvoke(true);
            Assert.Equal(true, re);

            //byte=>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte), typeof(bool));
            re = dd.DynamicInvoke((byte)1);
            Assert.Equal(true, re);

            // sbyte  => bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte), typeof(bool));
            re = dd.DynamicInvoke((sbyte)1);
            Assert.Equal(true, re);

            //short => bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short), typeof(bool));
            re = dd.DynamicInvoke((short)1);
            Assert.Equal(true, re);

            //ushort=>bool 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort), typeof(bool));
            re = dd.DynamicInvoke((ushort)1);
            Assert.Equal(true, re);

            //uint=> bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint), typeof(bool));
            re = dd.DynamicInvoke((uint)1);
            Assert.Equal(true, re);

            //int=>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int), typeof(bool));
            re = dd.DynamicInvoke((int)1);
            Assert.Equal(true, re);

            //long=>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long), typeof(bool));
            re = dd.DynamicInvoke((long)1);
            Assert.Equal(true, re);

            //ulong=>bool
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong), typeof(bool));
            re = dd.DynamicInvoke((ulong)1);
            Assert.Equal(true, re);

            //float=>bool 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float), typeof(bool));
            re = dd.DynamicInvoke((float)1);
            Assert.Equal(true, re);

            //double=>bool 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double), typeof(bool));
            re = dd.DynamicInvoke((double)1);
            Assert.Equal(true, re);

        }

        /// <summary>
        /// 测试类型转换，可空数值转可空数值
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNullableNumberToNullableNumber()
        {
            //数值转数值

            //bool? =>byte?
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(byte?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((byte?)1, re);

            //bool? =>sbyte?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(sbyte?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((sbyte?)1, re);

            //bool?=>short?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(short?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((short?)1, re);

            //bool? =>ushort?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(ushort?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((ushort?)1, re);

            //bool? =>uint?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(uint?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((uint?)1, re);

            //bool? =>int?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(int?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((int?)1, re);

            //bool? =>long?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(long?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((long?)1, re);

            //bool? =>ulong?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(ulong?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((ulong?)1, re);

            //bool? =>float?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(float?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((float?)1, re);

            //bool? =>double?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(double?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal((double?)1, re);
            //开始反转
            //bool?=>bool ?

            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(bool?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal(true, re);

            //byte?=>bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte?), typeof(bool?));
            re = dd.DynamicInvoke((byte?)1);
            Assert.Equal(true, re);

            // sbyte ? => bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte?), typeof(bool?));
            re = dd.DynamicInvoke((sbyte?)1);
            Assert.Equal(true, re);

            //short? => bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short?), typeof(bool?));
            re = dd.DynamicInvoke((short?)1);
            Assert.Equal(true, re);

            //ushort?=>bool ?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort?), typeof(bool?));
            re = dd.DynamicInvoke((ushort?)1);
            Assert.Equal(true, re);

            //uint?=> bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint?), typeof(bool?));
            re = dd.DynamicInvoke((uint?)1);
            Assert.Equal(true, re);

            //int?=>bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int?), typeof(bool?));
            re = dd.DynamicInvoke((int?)1);
            Assert.Equal(true, re);

            //long?=>bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long?), typeof(bool?));
            re = dd.DynamicInvoke((long?)1);
            Assert.Equal(true, re);

            //ulong?=>bool?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong?), typeof(bool?));
            re = dd.DynamicInvoke((ulong?)1);
            Assert.Equal(true, re);

            //float?=>bool ?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float?), typeof(bool?));
            re = dd.DynamicInvoke((float)1);
            Assert.Equal(true, re);

            //double=>bool 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double), typeof(bool));
            re = dd.DynamicInvoke((double?)1);
            Assert.Equal(true, re);
        }

        /// <summary>
        /// 测试类型转换，数值转数值,主要是decimal
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNumberToNumberSpecifyDecimal()
        {
            //数值转数值

            //decimal =>bool
            Delegate dd; object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(bool));
            re = dd.DynamicInvoke((decimal)1);
            Assert.Equal(true, re);

            //decimal =>byte
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(byte));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((byte)1, re);

            //decimal =>sbyte
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(sbyte));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((sbyte)1, re);

            //decimal =>short
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(short));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((short)1, re);

            //decimal =>ushort
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(ushort));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((ushort)1, re);

            //decimal =>uint
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(uint));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((uint)1, re);

            //decimal =>int
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(int));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((int)1, re);

            //decimal =>long
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(long));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((long)1, re);

            //decimal =>ulong
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(ulong));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((ulong)1, re);

            //decimal =>float
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(float));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((float)1, re);

            //decimal =>double
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal), typeof(double));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((double)1, re);

            //开始反转
            //bool=>decimal 

            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool), typeof(decimal));
            re = dd.DynamicInvoke(true);
            Assert.Equal(1m, re);

            //byte=>decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte), typeof(decimal));
            re = dd.DynamicInvoke((byte)1);
            Assert.Equal(1m, re);

            // sbyte  => decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte), typeof(decimal));
            re = dd.DynamicInvoke((sbyte)1);
            Assert.Equal(1m, re);

            //short => decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short), typeof(decimal));
            re = dd.DynamicInvoke((short)1);
            Assert.Equal(1m, re);

            //ushort=>decimal 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort), typeof(decimal));
            re = dd.DynamicInvoke((ushort)1);
            Assert.Equal(1m, re);

            //uint=> decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint), typeof(decimal));
            re = dd.DynamicInvoke((uint)1);
            Assert.Equal(1m, re);

            //int=>decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int), typeof(decimal));
            re = dd.DynamicInvoke((int)1);
            Assert.Equal(1m, re);

            //long=>decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long), typeof(decimal));
            re = dd.DynamicInvoke((long)1);
            Assert.Equal(1m, re);

            //ulong=>decimal
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong), typeof(decimal));
            re = dd.DynamicInvoke((ulong)1);
            Assert.Equal(1m, re);

            //float=>decimal 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float), typeof(decimal));
            re = dd.DynamicInvoke((float)1);
            Assert.Equal(1m, re);

            //double=>decimal 
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double), typeof(decimal));
            re = dd.DynamicInvoke((double)1);
            Assert.Equal(1m, re);
        }

        /// <summary>
        /// 测试类型转换，可空数值转可空数值,主要是decimal
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNullableNumberToNullableNumberSpecifyDecimal()
        {
            //数值转数值
            //decimal? =>bool?
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(bool?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal(true, re);

            //decimal? =>byte?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(byte?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((byte?)1, re);

            //decimal? =>sbyte?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(sbyte?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((sbyte?)1, re);

            //decimal ?=>short?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(short?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((short?)1, re);

            //decimal? =>ushort?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(ushort?));
            re = dd.DynamicInvoke(1m);
            Assert.Equal((ushort?)1, re);

            //decimal ?=>uint?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(uint?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((uint?)1, re);

            //decimal? =>int?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(int?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((int?)1, re);

            //decimal? =>long?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(long?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((long?)1, re);

            //decimal? =>ulong?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(ulong?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((ulong?)1, re);

            //decimal? =>float?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(float?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((float?)1, re);

            //decimal? =>double?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(decimal?), typeof(double?));
            re = dd.DynamicInvoke((decimal?)1);
            Assert.Equal((double?)1, re);

            //开始反转
            //bool?=>decimal ?

            dd = BuildConvertTypeToTargetTypeDelegate(typeof(bool?), typeof(decimal?));
            re = dd.DynamicInvoke((bool?)true);
            Assert.Equal(1m, re);

            //byte?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(byte?), typeof(decimal?));
            re = dd.DynamicInvoke((byte?)1);
            Assert.Equal(1m, re);

            // sbyte?  => decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(sbyte?), typeof(decimal?));
            re = dd.DynamicInvoke((sbyte?)1);
            Assert.Equal(1m, re);

            //short? => decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(short?), typeof(decimal?));
            re = dd.DynamicInvoke((short?)1);
            Assert.Equal(1m, re);

            //ushort?=>decimal ?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ushort?), typeof(decimal?));
            re = dd.DynamicInvoke((ushort?)1);
            Assert.Equal(1m, re);

            //uint?=> decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(uint?), typeof(decimal?));
            re = dd.DynamicInvoke((uint?)1);
            Assert.Equal(1m, re);

            //int?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(int?), typeof(decimal?));
            re = dd.DynamicInvoke((int?)1);
            Assert.Equal(1m, re);

            //long?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long?), typeof(decimal?));
            re = dd.DynamicInvoke((long?)1);
            Assert.Equal(1m, re);

            //ulong?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(ulong?), typeof(decimal?));
            re = dd.DynamicInvoke((ulong?)1);
            Assert.Equal(1m, re);

            //float?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(float?), typeof(decimal?));
            re = dd.DynamicInvoke((float?)1);
            Assert.Equal(1m, re);

            //double?=>decimal?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(double?), typeof(decimal?));
            re = dd.DynamicInvoke((double?)1);
            Assert.Equal(1m, re);
        }

        /// <summary>
        /// 测试类型转换，dateTime与其他类型互转
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeDatetimeAndOther()
        {
            //dateTime
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-AS");
            //string=>dateTime
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(DateTime));
            re = dd.DynamicInvoke("2022-10-28 16:00:00");
            Assert.Equal(new DateTime(2022, 10, 28, 16, 0, 0), re);

            //dateTime=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(DateTime), typeof(string));
            re = dd.DynamicInvoke(new DateTime(2022, 10, 28, 16, 0, 0));
            Assert.Equal("10/28/2022 16:00:00", re);
        }

        /// <summary>
        /// 测试类型转换，可空dateTime与其他类型互转
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNullableDatetimeAndOther()
        {
            //dateTime
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-AS");
            //string=>dateTime?
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(DateTime?));
            re = dd.DynamicInvoke("2022-10-28 16:00:00");
            Assert.Equal(new DateTime(2022, 10, 28, 16, 0, 0), re);

            //dateTime?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(DateTime?), typeof(string));
            re = dd.DynamicInvoke((DateTime?)new DateTime(2022, 10, 28, 16, 0, 0));
            Assert.Equal("10/28/2022 16:00:00", re);
        }



        /// <summary>
        /// 测试类型转换，枚举与其他类型互转
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeEnumAndOther()
        {
            //dateTime
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-AS");
            //enum=>long
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(IlEnum), typeof(long));
            re = dd.DynamicInvoke(IlEnum.a);
            Assert.Equal(1L, re);
            //long=>enum
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long), typeof(IlEnum));
            re = dd.DynamicInvoke(1L);
            Assert.Equal(IlEnum.a, re);

            //enum=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(IlEnum), typeof(string));
            re = dd.DynamicInvoke(IlEnum.a);
            Assert.Equal("a", re);

            //string=>enum
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(IlEnum));
            re = dd.DynamicInvoke("a");
            Assert.Equal(IlEnum.a, re);

            //string=>enum
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(IlEnum));
            re = dd.DynamicInvoke("1");
            Assert.Equal(IlEnum.a, re);
        }

        /// <summary>
        /// 测试类型转换，可空枚举与其他类型互转
        /// </summary>
        [Fact]
        public static void TestConvertTypeToTargetTypeNullableEnumAndOther()
        {
            //dateTime
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-AS");
            //enum?=>long?
            Delegate dd;
            object re;
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(IlEnum?), typeof(long?));
            re = dd.DynamicInvoke((IlEnum?)IlEnum.a);
            Assert.Equal(1L, re);
            //long?=>enum?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(long?), typeof(IlEnum?));
            re = dd.DynamicInvoke((long?)1L);
            Assert.Equal(IlEnum.a, re);

            //enum?=>string
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(IlEnum?), typeof(string));
            re = dd.DynamicInvoke((IlEnum?)IlEnum.a);
            Assert.Equal("a", re);

            //string=>enum?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(IlEnum?));
            re = dd.DynamicInvoke("a");
            Assert.Equal(IlEnum.a, re);

            //string=>enum?
            dd = BuildConvertTypeToTargetTypeDelegate(typeof(string), typeof(IlEnum?));
            re = dd.DynamicInvoke("1");
            Assert.Equal(IlEnum.a, re);
        }

        public static Delegate BuildConvertTypeToTargetTypeDelegate(Type fromType, Type toType)
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), toType,
                new Type[] { fromType });
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Box, fromType);
            //il.Emit(OpCodes.Call,typeof(int).GetMethod(nameof(int.ToString),Type.EmptyTypes));
            il.ConvertTypeToTargetType(fromType, toType);
            il.Emit(OpCodes.Ret);
            var funcType = Expression.GetFuncType(fromType, toType);
            var dd = dynamicMethod.CreateDelegate(funcType);
            return dd;
            ;
            //Assert.Equal(typeof(IlPerson), re);
        }
    }
}