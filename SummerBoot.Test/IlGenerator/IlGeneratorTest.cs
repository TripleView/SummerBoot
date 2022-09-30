using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using System.Threading.Tasks;
using System;
using Xunit;
using System.Diagnostics;
using System.Reflection.Emit;
using SummerBoot.Core;

namespace SummerBoot.Test.IlGenerator
{
    public class IlGeneratorTest
    {
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
            il.SteadLocal(ret);
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
    }
}