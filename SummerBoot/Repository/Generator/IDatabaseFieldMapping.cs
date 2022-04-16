using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.Generator
{
    public interface IDatabaseFieldMapping
    {
        List<string> ConvertDatabaseTypeToCsharpType(List<string> databaseTypeList);
        List<string> ConvertCsharpTypeToDatabaseType(List<string> csharpTypeList);
    }
}