using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 替换本地变量
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="localBuilder"></param>
        public static void SteadOfLocal(this ILGenerator ilGenerator, LocalBuilder localBuilder)
        {
            var localIndex = localBuilder.LocalIndex;
            switch (localIndex)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Stloc_0);
                    return;
                case 1:
                    ilGenerator.Emit(OpCodes.Stloc_1);
                    return;
                case 2:
                    ilGenerator.Emit(OpCodes.Stloc_2);
                    return;
                case 3:
                    ilGenerator.Emit(OpCodes.Stloc_3);
                    return;
            }

            if (localIndex > 0 && localIndex <= 255)
            {
                ilGenerator.Emit(OpCodes.Stloc_S, localIndex);
                return;
            }

            ilGenerator.Emit(OpCodes.Stloc, localIndex);
        }

        /// <summary>
        /// 加载本地变量
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="localBuilder"></param>
        public static void LoadLocal(this ILGenerator ilGenerator, LocalBuilder localBuilder)
        {
            var localIndex = localBuilder.LocalIndex;
            switch (localIndex)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    return;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    return;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldloc_2);
                    return;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldloc_3);
                    return;
            }

            if (localIndex > 0 && localIndex <= 255)
            {
                ilGenerator.Emit(OpCodes.Ldloc_S, localIndex);
                return;
            }

            ilGenerator.Emit(OpCodes.Ldloc, localIndex);
        }

        /// <summary>
        /// 加载int32位整数
        /// </summary>
        /// <param name="il"></param>
        /// <param name="value"></param>
        public static void EmitInt32(this ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte) value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }

                    break;
            }
        }


        /// <summary>
        /// 在il层面，将一种类型转换为另一种类型
        /// </summary>
        /// <param name="il"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        public static void ConvertTypeToTargetType(this ILGenerator il, Type fromType, Type toType)
        {
            var fromTypeNullableType = Nullable.GetUnderlyingType(fromType);
            var toTypeNullableType = Nullable.GetUnderlyingType(toType);
            var tempFromType = fromTypeNullableType ?? fromType;
            var tempToType = toTypeNullableType ?? toType;

            //校验是否已装箱，否则报错
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldtoken, fromType);
            il.Emit(OpCodes.Call,SbUtil.GetTypeFromHandleMethod);
            il.Emit(OpCodes.Call, CheckIsBoxMethod);
            //判断是否为枚举
            if (tempToType.IsEnum)
            {
                var enumUnderlyingType = Enum.GetUnderlyingType(tempToType);

                if (enumUnderlyingType != tempFromType)
                {
                    //string 类型可以转化
                    if (tempFromType == typeof(string))
                    {
                        //因为enum.parse这个方法，参数顺序是枚举类型type,具体的字符串value，是否忽略大小写bool,此时堆栈里的顺序与这个有点不一样，是string在前面，所以要先保存string，加载type后再加载string
                        var stringLocal = il.DeclareLocal(typeof(object));
                        il.SteadOfLocal(stringLocal);
                        //加载enum的元数据令牌
                        il.Emit(OpCodes.Ldtoken, tempToType);
                        //转为enum的type
                        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                        //加载string
                        il.LoadLocal(stringLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Call, EnumParse);
                        il.Emit(OpCodes.Unbox_Any, tempToType);
                        //如果是可控类型，则返回值要转为可空类型
                        if (toTypeNullableType != null)
                        {
                            il.Emit(OpCodes.Newobj,toType.GetConstructor(new []{tempToType}));
                        }
                        return;
                    }

                    il.Emit(OpCodes.Ldtoken, enumUnderlyingType);
                    il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                    il.Emit(OpCodes.Call, InvariantCulture);
                    il.Emit(OpCodes.Call, ConvertChangeTypeMethod);
                    il.Emit(OpCodes.Unbox_Any, tempToType);
                    //如果是可空类型，则返回值要转为可空类型
                    if (toTypeNullableType != null)
                    {
                        il.Emit(OpCodes.Newobj, toType.GetConstructor(new[] { tempToType }));
                    }
                    return;
                    //throw new Exception(
                    //    $"convert source type :{tempFromType.Name} to target type {tempToType.Name} error");
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, tempToType);
                    //如果是可控类型，则返回值要转为可空类型
                    if (toTypeNullableType != null)
                    {
                        il.Emit(OpCodes.Newobj,toType.GetConstructor(new []{tempToType}));
                    }
                    return;
                }
            }

            //判断是否类型一致，一致就直接拆箱
            if (tempFromType == tempToType)
            {
                il.Emit(OpCodes.Unbox_Any, toType);

                return;
            }

            //查找显式转换或者隐式转换方法
            var fromOpMethod = tempFromType.GetMethods().FirstOrDefault(it =>
                (it.Name == "op_Explicit" || it.Name == "op_Implicit") && it.ReturnType == tempToType &&
                it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == tempFromType);

            var toOpMethod = tempToType.GetMethods().FirstOrDefault(it =>
                (it.Name == "op_Explicit" || it.Name == "op_Implicit") && it.ReturnType == tempToType &&
                it.GetParameters().Length == 1 && it.GetParameters()[0].ParameterType == tempFromType);

            var opMethod = fromOpMethod ?? toOpMethod;
            if (opMethod != null)
            {
                il.Emit(OpCodes.Unbox_Any, tempFromType);
                il.Emit(OpCodes.Call, opMethod);
                //如果是可控类型，则返回值要转为可空类型
                if (toTypeNullableType != null)
                {
                    il.Emit(OpCodes.Newobj, toType.GetConstructor(new[] {tempToType}));
                }

                return;
            }


            //判断是否为数值类型之间的转换，因为数值类型之间的转换，il提供了原生的语法
            var isNumberConvert = false;
            OpCode opCode = default;
            if (tempFromType.IsNumberType() && tempToType.IsNumberType() && tempFromType != typeof(decimal) &&
                tempToType != typeof(decimal))
            {
                isNumberConvert = true;

                switch (Type.GetTypeCode(tempToType))
                {
                    case TypeCode.Byte:
                        opCode = OpCodes.Conv_Ovf_I1_Un;
                        break;
                    case TypeCode.SByte:
                        opCode = OpCodes.Conv_Ovf_I1;
                        break;
                    case TypeCode.UInt16:
                        opCode = OpCodes.Conv_Ovf_I2_Un;
                        break;
                    case TypeCode.Int16:
                        opCode = OpCodes.Conv_Ovf_I2;
                        break;
                    case TypeCode.UInt32:
                        opCode = OpCodes.Conv_Ovf_I4_Un;
                        break;
                    case TypeCode.Boolean:
                    case TypeCode.Int32:
                        opCode = OpCodes.Conv_Ovf_I4;
                        break;
                    case TypeCode.UInt64:
                        opCode = OpCodes.Conv_Ovf_I8_Un;
                        break;
                    case TypeCode.Int64:
                        opCode = OpCodes.Conv_Ovf_I8;
                        break;
                    case TypeCode.Single:
                        opCode = OpCodes.Conv_R4;
                        break;
                    case TypeCode.Double:
                        opCode = OpCodes.Conv_R8;
                        break;
                    default:
                        isNumberConvert = false;
                        break;
                }
            }

            //数值类型
            if (isNumberConvert)
            {
                il.Emit(OpCodes.Unbox_Any, tempFromType);
                il.Emit(opCode);
                if (tempToType == typeof(bool))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                }

                //如果是可空类型，则返回值要转为可空类型
                if (toTypeNullableType != null)
                {
                    il.Emit(OpCodes.Newobj, toType.GetConstructor(new[] {tempToType}));
                }
            }
            //其他类型转换
            else
            {
                il.Emit(OpCodes.Ldtoken, tempToType);
                il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                il.Emit(OpCodes.Call, InvariantCulture);
                il.Emit(OpCodes.Call, ConvertChangeTypeMethod);
                il.Emit(OpCodes.Unbox_Any, toType);
            }
        }

        /// <summary>
        /// 校验类型是否已装箱
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void CheckIsBox(object value, Type type)
        {
            var result = value?.GetType() == type;
        }
    }
}