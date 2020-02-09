using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;

public class TypeCreator
{
    private Type targetType;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="targetType">被实现或者继承的类型</param>
    public TypeCreator(Type targetType)
    {
        this.targetType = targetType;
    }

    public void Test()
    {
   Console.WriteLine("我草你打野");
    }

    public Type Build()
    {
        //System.Reflection.AssemblyName 是用来表示一个Assembly的完整名称的
        AssemblyName assyName = new AssemblyName();
        //为要创建的Assembly定义一个名称（这里忽略版本号，Culture等信息）
        assyName.Name = "MyAssyFor_" + targetType.Name;
        //获取AssemblyBuilder
        //AssemblyBuilderAccess有Run，Save，RunAndSave三个取值
        AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
      
        //获取ModuleBuilder，提供String参数作为Module名称，随便设一个
        ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule("MyModFor_" + targetType.Name);
        //新类型的名称：随便定一个
        String newTypeName = "Imp_" + targetType.Name;
        //新类型的属性：要创建的是Class，而非Interface，Abstract Class等，而且是Public的
        TypeAttributes newTypeAttribute = TypeAttributes.Class | TypeAttributes.Public;
        //声明要创建的新类型的父类型
        Type newTypeParent;
        //声明要创建的新类型要实现的接口
        Type[] newTypeInterfaces;
        //对于基类型是否为接口，作不同处理
        if (targetType.IsInterface)
        {
            newTypeParent = typeof(object);
            newTypeInterfaces = new Type[] { targetType };
            
        }
        else
        {
            newTypeParent = targetType;
            newTypeInterfaces = Type.EmptyTypes;
        }
        //得到类型生成器            
        TypeBuilder typeBuilder = modBuilder.DefineType(newTypeName, newTypeAttribute, newTypeParent, newTypeInterfaces);

        /*
         * 构造函数 实例化 ModifiedPropertyNames,生成类似于下面的代码
           ModifiedPropertyNames = new List<string>();
        */
        ConstructorBuilder constructorBuilder =
            typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
        ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
        ilgCtor.Emit(OpCodes.Ldarg_0); //加载当前类
        ilgCtor.Emit(OpCodes.Newobj, newTypeParent.GetConstructor(new Type[0])); //实例化对象入栈
        ilgCtor.Emit(OpCodes.Stfld, fbModifiedPropertyNames); //设置fbModifiedPropertyNames值,为刚入栈的实例化对象
        ilgCtor.Emit(OpCodes.Ret); //返回

        //typeBuilder.AddInterfaceImplementation(targetType);
        //以下将为新类型声明方法：新类型应该override基类型的所以virtual方法
        //得到基类型的所有方法
        MethodInfo[] targetMethods = targetType.GetMethods();
        //遍历各个方法，对于Virtual的方法，获取其签名，作为新类型的方法
        foreach (MethodInfo targetMethod in targetMethods)
        {
            //只挑出virtual的方法
            if (targetMethod.IsVirtual)
            {
                //得到方法的各个参数的类型
                ParameterInfo[] paramInfo = targetMethod.GetParameters();
                Type[] paramType = new Type[paramInfo.Length];
                for (int i = 0; i < paramInfo.Length; i++)
                    paramType[i] = paramInfo[i].ParameterType;
                //传入方法签名，得到方法生成器
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, targetMethod.ReturnType, paramType);
                //由于要生成的是具体类，所以方法的实现是必不可少的。而方法的实现是通过Emit IL代码来产生的
                //得到IL生成器
                ILGenerator ilGen = methodBuilder.GetILGenerator();


                //以下三行相当于：{Console.Writeln("I'm "+ targetMethod.Name +"ing");}
                ilGen.Emit(OpCodes.Ldstr, "I'm " + targetMethod.Name + "ing");
                ilGen.Emit(OpCodes.Call, typeof(TypeCreator).GetMethod("Test", Type.EmptyTypes));
               
                // pop the stack if return void
                if (targetMethod.ReturnType == typeof(void))
                {
                    ilGen.Emit(OpCodes.Pop);
                }

                // complete
                ilGen.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder,targetMethod );
            }
        }

        return typeBuilder.CreateTypeInfo().AsType();
        //真正创建，并返回
        //return (typeBuilder.CreateType());
    }
}