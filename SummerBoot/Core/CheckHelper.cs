using System;

namespace SummerBoot.Core;

public static class CheckHelper
{
    public static void NotNull<T>(T obj,string parameterName)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}