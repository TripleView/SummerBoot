using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using System.Threading.Tasks;
using System;
using System.Data;
using Xunit;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Test.IlGenerator.Dto;
using BindingFlags = System.Reflection.BindingFlags;

namespace SummerBoot.Test.IlGenerator
{
    public class IlGeneratorTest
    {
        /// <summary>
        /// 测试try catch
        /// </summary>
        [Fact]
        public static void TestTryCatch2()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[]{typeof(string)});
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
            var dd = (Func<string,int>)dynamicMethod.CreateDelegate(typeof(Func<string,int>));
            var re = dd("123");
            //Assert.Equal("何泽平", re);
        }
        /// <summary>
        /// 测试try catch
        /// </summary>
        [Fact]
        public static void TestTryCatch()
        {
            Assert.Equal(true, typeof(IlValueTypeItem).IsValueType);

            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(string),
                Type.EmptyTypes);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "何泽平");
            il.BeginExceptionBlock();
            il.Emit(OpCodes.Ldstr, "0");
            il.Emit(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ToInt32), new Type[] { typeof(string) }));
            il.Emit(OpCodes.Pop);
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            il.EndExceptionBlock();
            il.Emit(OpCodes.Ret);
            var dd = (Func<string>)dynamicMethod.CreateDelegate(typeof(Func<string>));
            var re = dd();
            Assert.Equal("何泽平", re);
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
        /// 测试反射获取属性
        /// </summary>
        [Fact]
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
                Name varchar(100) NULL,
                Age INT NULL
                    )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"INSERT INTO test (Name,Age) VALUES ('何泽平',30)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "select * from test";
                IDataReader dr = cmd.ExecuteReader();
                DatabaseContext.GetTypeDeserializer(typeof(IlPerson),dr);

                conn.Close();
            }

            var p1 = typeof(IlResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.Equal(1, p1.Length);
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
            var p1 = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance ).FirstOrDefault(it=>it.Name=="get_Item"&& it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == typeof(int));
                //;
            Assert.NotNull(p1);
        }

        public T Test<T>()
        {
            return default;
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
            var nameProperty= typeof(IlValueTypeItem).GetProperty("Name");
            var ctor = typeof(IlResult).GetConstructor(Type.EmptyTypes);
        
            il.Emit(OpCodes.Ldloca, ilRe);

            il.Emit(OpCodes.Initobj, typeof(IlValueTypeItem)); //stack is 

            il.Emit(OpCodes.Ldloca,ilRe);
            il.Emit(OpCodes.Ldstr,"何泽平");
            il.Emit(OpCodes.Call,nameProperty.GetSetMethod());
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box,typeof(IlValueTypeItem));
            il.Emit(OpCodes.Ret);

            var dd = (Func<object>) dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re =(IlValueTypeItem) dd();
             Assert.Equal("何泽平",re.Name);
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
          
            il.Emit(OpCodes.Box,typeof(int));
            il.Emit(OpCodes.Ret);
            var cccc = typeof(IlResult).GetConstructors();
   
            var dd = (Func<object>) dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = dd();
            Assert.Equal(1,re);
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
            var dd = (Func<object>) dynamicMethod.CreateDelegate(typeof(Func<object>));
            var re = (IlResult) dd();
            Assert.Equal("何泽平", re.Name);
        }


        /// <summary>
        /// 测试常规加减法
        /// </summary>
        [Fact]
        public static void TestAdd()
        {
            var dynamicMethod = new DynamicMethod("test" + Guid.NewGuid().ToString("N"), typeof(int),
                new Type[] {typeof(int), typeof(int)});
            var il = dynamicMethod.GetILGenerator();
            var ret = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Add);
            il.SteadOfLocal(ret);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            var dd = (Func<int, int, int>) dynamicMethod.CreateDelegate(typeof(Func<int, int, int>));
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
                new Type[] {typeof(int)});
            var debugWriteMethod = typeof(Debug)
                .GetMethod(nameof(Debug.WriteLine), new Type[] {typeof(object)});
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
            var func = (Func<int, string>) dynamicMethod.CreateDelegate(typeof(Func<int, string>));
            var re = func(5);
            Assert.Equal("相等", re);
            var re2 = func(6);
            Assert.Equal("不相等", re2);
        }
    }
}