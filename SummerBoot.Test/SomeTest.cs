using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using ExpressionParser.Test;
using SummerBoot.Core;
using SummerBoot.Test.Model;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Repository.Core;
using System.Data;
using SummerBoot.Test.IlGenerator;

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

    public static class StaticClass
    {
        private static ITypeHandler<int> Int3;
        public static int Test(int value)
        {
            Int3.SetValue(null,value);
            return 3;
        }
    }

    public class SomeTest
    {
        public interface IDynamicGenerateInterface
        {
            void Write(string a);
        }
        public interface IDynamicGenerateInterface2 : IDynamicGenerateInterface
        {
        }
        public class DynamicGenerateInterface : IDynamicGenerateInterface
        {
            public void Write(string a)
            {
                Debug.WriteLine(a);
            }
        }

        public class SecondDynamicGenerateInterface
        {
            private readonly IDynamicGenerateInterface dynamicGenerateInterface;

            public SecondDynamicGenerateInterface(IDynamicGenerateInterface dynamicGenerateInterface)
            {
                this.dynamicGenerateInterface = dynamicGenerateInterface;
            }

            public void Write(string a)
            {
                dynamicGenerateInterface.Write(a);
            }
        }

        public class Three : SecondDynamicGenerateInterface
        {
            public Three(IDynamicGenerateInterface2 a2) : base(a2)
            {

            }
        }

        /// <summary>
        /// 动态生成静态类，并添加2个方法,实验结论Ldsfld可以直接emit一个FieldBuilder,static方法中Ldarg_0为参数，class的方法里，则为class本身
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="constructorType"></param>
        /// <returns></returns>
        [Fact]
        public void GenerateStaticClass()
        {
            var c = typeof(StaticClass).Attributes;
            var name = "ITest";
            string assemblyName = name + "ProxyAssembly";
            string moduleName = name + "ProxyModule";
            string typeName = name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public | TypeAttributes.Abstract |
                                              TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = null;

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("GenerateStaticClass", newTypeAttribute, parentType, interfaceTypes);

            var typeParams = typeBuilder.DefineGenericParameters("T");

            GenericTypeParameterBuilder first = typeParams[0];
            //first.SetGenericParameterAttributes(
            //    GenericParameterAttributes.ReferenceTypeConstraint);

            var staticIntField = typeBuilder.DefineField("intF", first,
                FieldAttributes.Public | FieldAttributes.Static);

            var staticGetMethod = typeBuilder.DefineMethod("Get", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, first, new Type[] { typeof(int), typeof(int) });
            var ilG = staticGetMethod.GetILGenerator();
            ilG.Emit(OpCodes.Ldsfld, staticIntField);
            ilG.Emit(OpCodes.Ldarg_0);
            ilG.Emit(OpCodes.Add);
            ilG.Emit(OpCodes.Ldarg_1);
            ilG.Emit(OpCodes.Add);
            ilG.Emit(OpCodes.Ret);

            var staticSetMethod = typeBuilder.DefineMethod("Set", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, null, new Type[] { first });
            var ilG2 = staticSetMethod.GetILGenerator();
            ilG2.Emit(OpCodes.Ldarg_0);
            ilG2.Emit(OpCodes.Stsfld, staticIntField);
            ilG2.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            resultType = resultType.MakeGenericType(typeof(int));
            resultType.GetMethod("Set").Invoke(null, new object[1] { 3 });
            var d = (int)resultType.GetMethod("Get").Invoke(null, new object[] { 1, 2 });
            Assert.Equal(6,d);
        }



        /// <summary>
        /// 测试动态生成interface并且注入到ioc容器里
        /// </summary>
        [Fact]
        public void TestDynamicGenerateInterfaceAndInsertDependency()
        {
            var name = "ITest";
            string assemblyName = name + "ProxyAssembly";
            string moduleName = name + "ProxyModule";
            string typeName = name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);

            var interface1 = GenerateInterface(modBuilder);
            var class1 = GenerateClass(modBuilder, interface1);
            var child = GenerateClassWithSpecialConstructor(modBuilder, interface1);

            ServiceCollection services = new ServiceCollection();
            services.AddScoped(interface1, class1);
            services.AddScoped(child);
            var pro = services.BuildServiceProvider();
            var c = (IDynamicGenerateInterface)pro.GetRequiredService(interface1);
            c.Write("456");
            var d = (SecondDynamicGenerateInterface)pro.GetRequiredService(child);

            //(IDynamicGenerateInterface)
            d.Write("789");
        }

        public Type GenerateClassWithSpecialConstructor(ModuleBuilder modBuilder, Type constructorType)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = typeof(SecondDynamicGenerateInterface);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("GenerateClassWithSpecialConstructor", newTypeAttribute, parentType, interfaceTypes);

            var parentConstruct = parentType.GetConstructors().FirstOrDefault();

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                 new Type[] { constructorType });
            var conIl = constructor.GetILGenerator();
            conIl.Emit(OpCodes.Ldarg_0);
            conIl.Emit(OpCodes.Ldarg_1);
            conIl.Emit(OpCodes.Call, parentConstruct);
            conIl.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        public Type GenerateInterface(ModuleBuilder modBuilder)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public |
                                              TypeAttributes.Interface |
                                              TypeAttributes.Abstract |
                                              TypeAttributes.AutoClass |
                                              TypeAttributes.AnsiClass |
                                              TypeAttributes.BeforeFieldInit |
                                              TypeAttributes.AutoLayout;
            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = typeof(IDynamicGenerateInterface);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("IProxy", newTypeAttribute, null, new Type[] { typeof(IDynamicGenerateInterface) });
            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }

        public Type GenerateClass(ModuleBuilder modBuilder, Type interface1)
        {
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Class | TypeAttributes.Public;
            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = new Type[] { interface1 };
            parentType = typeof(DynamicGenerateInterface);

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("Proxy", newTypeAttribute, parentType, interfaceTypes);
            var resultType = typeBuilder.CreateTypeInfo().AsType();
            return resultType;
        }
        /// <summary>
        /// 测试动态生成interface并且注入到ioc容器里
        /// </summary>


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
        public void TestCompareGetPropertyValueByEmitOrExpression()
        {
            var dog = new Dog()
            {
                Name = "sb"
            };
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                var value = dog.GetPropertyValueByEmit("Name");
            }
            sw.Stop();
            var time1 = sw.ElapsedMilliseconds;
            //sw.Restart();
            //for (int i = 0; i < 1000000; i++)
            //{
            //    var value = dog.GetPropertyValue("Name", pro, type);
            //}
            //sw.Stop();
            //var time2 = sw.ElapsedMilliseconds;
            //var diff = time1 - time2;
        }


        [Fact]
        public void TestGetPropertyValueByEmit()
        {
            var dog = new Dog()
            {
                Name = "sb",
                Active = null
            };
            var value = dog.GetPropertyValueByEmit<Dog, string>("Name");
            Assert.Equal("sb", value);
            var intValue = dog.GetPropertyValueByEmit<Dog, int?>("Active");
            Assert.Equal(null, intValue);
            dog.Active = 1;
            intValue = dog.GetPropertyValueByEmit<Dog, int?>("Active");
            Assert.Equal(1, intValue);
            dog.Active = null;
            var value2 = dog.GetPropertyValueByEmit("Name");
            Assert.Equal("sb", value2.ToString());
            var intValue2 = dog.GetPropertyValueByEmit<Dog, int?>("Active");
            Assert.Equal(null, intValue2);
            dog.Active = 1;
            intValue2 = dog.GetPropertyValueByEmit<Dog, int?>("Active");
            Assert.Equal(1, intValue2);
        }

        [Fact]
        public void TestStructGetPropertyValueByEmit()
        {
            var dogStruct3 = new DogStruct3()
            {
                Name = "sb",
                Age = 1
            };

            var value3 = dogStruct3.GetPropertyValueByEmit(nameof(dogStruct3.Name));
            var value = dogStruct3.GetPropertyValueByEmit<DogStruct3, string>(nameof(dogStruct3.Name));
            Assert.Equal("sb", value);
            var value2 = dogStruct3.GetPropertyValueByEmit<DogStruct3, int?>(nameof(dogStruct3.Age));
            Assert.Equal("sb", value);
            Assert.Equal(1, value2);
            //var value3 = dogStruct3.GetPropertyValueByEmit(nameof(dogStruct3.Name));
            Assert.Equal("sb", value3.ToString());
            var value4 = dogStruct3.GetPropertyValueByEmit(nameof(dogStruct3.Age));
            Assert.Equal(1, value4);
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
                var cls = Activator.CreateInstance(type, args: new object[1] { "abc" });
                list1.Add(cls);
            }
            sw.Stop();
            var l1 = sw.ElapsedMilliseconds;
            var func = SbUtil.BuildGenerateObjectDelegate(type.GetConstructors().FirstOrDefault());
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var cls = func.DynamicInvoke("abc");
                list2.Add(cls);
            }
            sw.Stop();
            var l2 = sw.ElapsedMilliseconds;
            var diff = l1 - l2;
        }
    }
}