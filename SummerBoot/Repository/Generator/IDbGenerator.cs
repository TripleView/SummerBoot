using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.Generator
{
    public interface IDbGenerator
    {
        List<string> GenerateSql(List<Type> types);
        List<string> GenerateCsharpClass(List<string> tableNames, string classNameSpace);
    }
}