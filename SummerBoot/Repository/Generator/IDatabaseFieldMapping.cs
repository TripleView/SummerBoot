using System;
using System.Collections.Generic;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public interface IDatabaseFieldMapping
    {
        List<string> ConvertDatabaseTypeToCsharpType(List<DatabaseFieldInfoDto> databaseFieldInfoList);
        List<string> ConvertCsharpTypeToDatabaseType(List<string> csharpTypeList);
    }
}