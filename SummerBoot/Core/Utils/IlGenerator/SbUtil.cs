using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Reflection.Emit;

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
            return;
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
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }
    }
}