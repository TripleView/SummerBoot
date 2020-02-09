using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace demo.Service
{
    public class DynamicProxyGenerator
    {

        private const string DynamicAssemblyName = "DynamicAssembly"; //动态程序集名称
        private const string DynamicModuleName = "DynamicAssemblyModule";
        private const string DynamicModuleDllName = "DynamicAssembly.dll"; //动态模块名称
        private const string ProxyClassNameFormater = "{0}Proxy";
        private const string ModifiedPropertyNamesFieldName = "ModifiedPropertyNames";

        private const MethodAttributes GetSetMethodAttributes =
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.CheckAccessOnOverride |
            MethodAttributes.Virtual | MethodAttributes.HideBySig;

        /// <summary>
        /// 创建动态程序集,返回AssemblyBuilder
        /// </summary>
        /// <param name="isSavaDll"></param>
        /// <returns></returns>
        private static AssemblyBuilder DefineDynamicAssembly(bool isSavaDll = false)
        {
            //动态创建程序集
            AssemblyName DemoName = new AssemblyName(DynamicAssemblyName);
            AssemblyBuilderAccess assemblyBuilderAccess =
                isSavaDll ? AssemblyBuilderAccess.RunAndCollect : AssemblyBuilderAccess.Run;
            ////AssemblyBuilder dynamicAssembly =
            ////    AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName, assemblyBuilderAccess);
            //AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.CreateInstance(DynamicAssemblyName,"123");
            //    System.ServiceModel.ChannelFactory
            //return dynamicAssembly;
            var result= AssemblyBuilder.DefineDynamicAssembly(DemoName,
                AssemblyBuilderAccess.Run);
            return result;
        }

        /// <summary>
        /// 创建动态模块,返回ModuleBuilder
        /// </summary>
        /// <param name="isSavaDll"></param>
        /// <returns>ModuleBuilder</returns>
        private static ModuleBuilder DefineDynamicModule(AssemblyBuilder dynamicAssembly, bool isSavaDll = false)
        {
            ModuleBuilder moduleBuilder = null;
          
                moduleBuilder = dynamicAssembly.DefineDynamicModule(DynamicModuleName);
            return moduleBuilder;
        }

        public void Test()
        {
            Console.WriteLine("123");
        }
        /// <summary>
        /// 创建动态代理类,重写属性Get Set 方法,并监控属性的Set方法,把变更的属性名加入到list集合中,需要监控的属性必须是virtual
        /// 如果你想保存修改的属性名和属性值,修改Set方法的IL实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSavaDynamicModule"></param>
        /// <returns></returns>
        public static T CreateDynamicProxy<T>(bool isSavaDynamicModule = false)
        {
            Type modifiedPropertyNamesType = typeof(HashSet<string>);

            Type typeNeedProxy = typeof(T);
            AssemblyBuilder assemblyBuilder = DefineDynamicAssembly(isSavaDynamicModule);
            //动态创建模块
            ModuleBuilder moduleBuilder = DefineDynamicModule(assemblyBuilder, isSavaDynamicModule);
            string proxyClassName = string.Format(ProxyClassNameFormater, typeNeedProxy.Name);
            //动态创建类代理
            TypeBuilder typeBuilderProxy =
                moduleBuilder.DefineType(proxyClassName, TypeAttributes.Public, typeNeedProxy);
            //定义一个变量存放属性变更名
            FieldBuilder fbModifiedPropertyNames = typeBuilderProxy.DefineField(ModifiedPropertyNamesFieldName,
                modifiedPropertyNamesType, FieldAttributes.Public);

            /*
             * 构造函数 实例化 ModifiedPropertyNames,生成类似于下面的代码
               ModifiedPropertyNames = new List<string>();
            */
            //ConstructorBuilder constructorBuilder =
            //    typeBuilderProxy.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
            //ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
            //ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
            //ilgCtor.Emit(OpCodes.Newobj, modifiedPropertyNamesType.GetConstructor(new Type[0])); //实例化对象入栈
            //ilgCtor.Emit(OpCodes.Stfld, fbModifiedPropertyNames); //设置fbModifiedPropertyNames值,为刚入栈的实例化对象
            //ilgCtor.Emit(OpCodes.Ret); //返回

           var test= typeBuilderProxy.DefineMethod("Test", MethodAttributes.Public, null, null);
           var f= test.GetILGenerator();
           var fff = typeof(DynamicProxyGenerator).GetMethod("Test");
           f.Emit(opcode:OpCodes.Call, fff);
           f.Emit(OpCodes.Pop);
            //获取被代理对象的所有属性,循环属性进行重写
            PropertyInfo[] properties = typeNeedProxy.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                Type typePepropertyInfo = propertyInfo.PropertyType;
                //动态创建字段和属性
                FieldBuilder fieldBuilder =
                    typeBuilderProxy.DefineField("_" + propertyName, typePepropertyInfo, FieldAttributes.Private);
                PropertyBuilder propertyBuilder = typeBuilderProxy.DefineProperty(propertyName,
                    PropertyAttributes.SpecialName, typePepropertyInfo, null);

                //重写属性的Get Set方法
                var methodGet = typeBuilderProxy.DefineMethod("get_" + propertyName, GetSetMethodAttributes,
                    typePepropertyInfo, Type.EmptyTypes);
                var methodSet = typeBuilderProxy.DefineMethod("set_" + propertyName, GetSetMethodAttributes, null,
                    new Type[] {typePepropertyInfo});

                //il of get method
                var ilGetMethod = methodGet.GetILGenerator();
                ilGetMethod.Emit(OpCodes.Ldarg_0);
                ilGetMethod.Emit(OpCodes.Ldfld, fieldBuilder);
                ilGetMethod.Emit(OpCodes.Ret);
                //il of set method
                ILGenerator ilSetMethod = methodSet.GetILGenerator();
                ilSetMethod.Emit(OpCodes.Ldarg_0);
                ilSetMethod.Emit(OpCodes.Ldarg_1);
                ilSetMethod.Emit(OpCodes.Stfld, fieldBuilder);
                ilSetMethod.Emit(OpCodes.Ldarg_0);
                ilSetMethod.Emit(OpCodes.Ldfld, fbModifiedPropertyNames);
                ilSetMethod.Emit(OpCodes.Ldstr, propertyInfo.Name);
                ilSetMethod.Emit(OpCodes.Callvirt,
                    modifiedPropertyNamesType.GetMethod("Add", new Type[] {typeof(string)}));
                ilSetMethod.Emit(OpCodes.Pop);
                ilSetMethod.Emit(OpCodes.Ret);

                //设置属性的Get Set方法
                propertyBuilder.SetGetMethod(methodGet);
                propertyBuilder.SetSetMethod(methodSet);
            }

            //使用动态类创建类型
            Type proxyClassType = typeBuilderProxy.CreateType();
            //保存动态创建的程序集
            //if (isSavaDynamicModule)
            //    assemblyBuilder.Save(DynamicModuleDllName);
            //创建类实例
            var instance = Activator.CreateInstance(proxyClassType);
            return (T) instance;
        }

        /// <summary>
        /// 获取属性的变更名称,
        /// 此处只检测调用了Set方法的属性,不会检测值是否真的有变
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HashSet<string> GetModifiedProperties(object obj)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(ModifiedPropertyNamesFieldName);
            if (fieldInfo == null) return null;
            object value = fieldInfo.GetValue(obj);
            return value as HashSet<string>;
        }
    }
}