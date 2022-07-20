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
            var dog = new Dog2()
            {
                Name = "sb",
                Active = 1,
                Enum2 = Enum2.y
            };

            var dogObj = SbUtil.BuildGenerateObjectDelegate(typeof(Dog2).GetConstructors().FirstOrDefault(it => it.GetParameters().Length == 3))
                .DynamicInvoke("sb", 1, Enum2.y);
            var buildDog = dogObj as Dog2;
            Assert.Equal("sb", buildDog.Name);
            Assert.Equal(1, buildDog.Active);
            Assert.Equal(Enum2.y, buildDog.Enum2);
            dogObj.SetPropertyValue("Name", "sb2");
            Assert.Equal("sb2", buildDog.Name);
        }


        [Fact]
        public void TestListToTable()
        {
            var dog = new Dog2()
            {
                Name = "sb",
                Active = 1,
                Enum2 = Enum2.y
            };
            var list = new List<Dog2>() { dog };
            var c = list.ToDataTable();
            var dog2 = new Dog2()
            {
                Name = "sb2",
                Active = null,
                Enum2 = Enum2.x
            };
            list.Add(dog2);
            var c2 = list.ToDataTable();

            Assert.Equal("sb", c2.Rows[0][0]);
            Assert.Equal(1, c2.Rows[0][1]);
            Assert.Equal((int)Enum2.y, c2.Rows[0][2]);
            Assert.Equal("sb2", c2.Rows[1][0]);
            Assert.Equal(DBNull.Value, c2.Rows[1][1]);
            Assert.Equal((int)Enum2.x, c2.Rows[1][2]);
        }

        [Fact]
        public void TestBuildObjectGetValuesDelegate()
        {
            var dog = new Dog2()
            {
                Name = "sb",
                Active = 1,
                Enum2 = Enum2.y
            };
            var lambda = SbUtil.BuildObjectGetValuesDelegate<Dog2>(dog.GetType().GetProperties().ToList());
            var result = lambda(dog);
            Assert.Equal("sb", result[0]);
            Assert.Equal(1, result[1]);
            Assert.Equal(Enum2.y, result[2]);
        }


        [Fact]
        public void TestArray()
        {
            var c = typeof(Dog).MakeArrayType(1);
            var d = (Array)Activator.CreateInstance(c, args: new object[] { 16 });
            var dffffff = typeof(Dog).GetProperty("Name").PropertyType;
            var dog = new Dog()
            {
                Name = "sb"
            };
            d.SetValue(dog, 0);
            Assert.Equal("sb", (d.GetValue(0) as Dog).Name);
        }

        [Fact]
        public void TestSetPropertyValueByExpression()
        {
            var dog = new Dog2()
            {
                Name = "sb"
            };
            dog.SetPropertyValue("Name", "sb2");
            dog.SetPropertyValue("Active", 1);
            dog.SetPropertyValue("Enum2", Enum2.y);
            Assert.Equal("sb2", dog.Name);
            Assert.Equal(1, dog.Active);

            Assert.Equal(Enum2.y, dog.Enum2);
            dog.SetPropertyValue("Active", null);
            Assert.Equal(null, dog.Active);
        }

        [Fact]
        public void TestGetPropertyValueByExpression()
        {
            var dog = new Dog()
            {
                Name = "sb",
                Active = null
            };
            var value = dog.GetPropertyValue<Dog, string>("Name");
            Assert.Equal("sb", value);
            var intValue = dog.GetPropertyValue<Dog, int?>("Active");
            Assert.Equal(null, intValue);
            dog.Active = 1;
            intValue = dog.GetPropertyValue<Dog, int?>("Active");
            Assert.Equal(1, intValue);
            dog.Active = null;
            var value2 = dog.GetPropertyValue("Name");
            Assert.Equal("sb", value2.ToString());
            var intValue2 = dog.GetPropertyValue<Dog, int?>("Active");
            Assert.Equal(null, intValue2);
            dog.Active = 1;
            intValue2 = dog.GetPropertyValue<Dog, int?>("Active");
            Assert.Equal(1, intValue2);
        }
        [Fact]
        public void TestGenerateObjectFast()
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