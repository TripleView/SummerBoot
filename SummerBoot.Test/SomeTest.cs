using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExpressionParser.Test;
using SummerBoot.Core;
using SummerBoot.Test.Model;
using Xunit;

namespace SummerBoot.Test
{
    public class TestCls
    {
        public TestCls(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
    public class SomeTest
    {
        [Fact]
        public void TestGenerateObject()
        {
            var type = typeof(TestCls);
            var sw = new Stopwatch();
            var list1 = new List<object>();
            var list2 = new List<object>();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
               var cls=  Activator.CreateInstance(type, args: new object[1] { "abc" });
                list1.Add(cls);
            }
            sw.Stop();
            var l1=sw.ElapsedMilliseconds;
            var func= SbUtil.BuildGenerateObjectDelegate(type.GetConstructors().FirstOrDefault());
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var cls= func.DynamicInvoke("abc");
                list2.Add(cls);
            }
            sw.Stop();
            var l2=sw.ElapsedMilliseconds;
            var diff = l1 - l2;
        }
    }
}