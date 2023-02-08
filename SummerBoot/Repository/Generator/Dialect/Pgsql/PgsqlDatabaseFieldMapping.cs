using System;
using System.Collections.Generic;
using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator.Dialect
{
    public class PgsqlDatabaseFieldMapping : IDatabaseFieldMapping
    {
        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","text"},//
            {"Int32","int4"},//
            {"Int64","int8"},//
            {"DateTime","timestamp"},//
            {"Decimal","numeric"},//
            {"Boolean","bool"},//
            {"Double","float8"},//
            {"Int16","int2"},//
            {"TimeSpan","interval"},//
            {"Guid","uuid"},//
            {"Byte","int2"},//
            {"Single","float4"},//
        };
        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
        {
            {"text","string"},
            {"varchar","string"},
            {"float4","float"},
            {"float8","double"},
            {"uuid","Guid"},//
            {"date","DateTime"},
            {"timestamp","DateTime"},
            {"int2","short"},//
            {"int4","int"},//
            {"int8","long"},//
            {"bool","bool"},//
            {"numeric","decimal"},//
            {"interval","TimeSpan"},//
        };


        public List<string> ConvertCsharpTypeToDatabaseType(List<string> csharpTypeList)
        {
            var result = new List<string>();
            foreach (var type in csharpTypeList)
            {
                var item = csharpTypeToDatabaseTypeMappings[type];
                result.Add(item);
            }

            return result;
        }

        public List<string> ConvertDatabaseTypeToCsharpType(List<DatabaseFieldInfoDto> databaseFieldInfoList)
        {
            var result = new List<string>();
            foreach (var databaseFieldInfo in databaseFieldInfoList)
            {
                var item = "";
                if (DatabaseTypeToCsharpTypeMappings.ContainsKey(databaseFieldInfo.ColumnDataType))
                {
                    item= DatabaseTypeToCsharpTypeMappings[databaseFieldInfo.ColumnDataType];
                }
                
                if (item.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("not support " + databaseFieldInfo.ColumnDataType);
                }
                
                result.Add(item);
            }

            return result;
        }

        private bool IsType(DatabaseFieldInfoDto dto,string dataTypeName)
        {
            if (dto.ColumnDataType.Contains(dataTypeName) && dto.ColumnDataType.Substring(0, dataTypeName.Length) == dataTypeName)
            {
                return true;
            }

            return false;
        }
    }
}