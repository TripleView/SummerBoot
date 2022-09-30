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
        public static void SteadLocal(this ILGenerator ilGenerator, LocalBuilder localBuilder)
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
    }
}