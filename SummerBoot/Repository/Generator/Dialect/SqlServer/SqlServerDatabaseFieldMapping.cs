using System;
using System.Collections.Generic;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator.Dialect
{
    public class SqlServerDatabaseFieldMapping : IDatabaseFieldMapping
    {
        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","nvarchar"},
            {"Int32","int"},
            {"Int64","bigint"},
            {"DateTime","datetime2"},
            {"Decimal","decimal"},
            {"Boolean","bit"},
            {"Byte[]","binary"},
            {"Double","float"},
            {"Int16","smallint"},
            {"TimeSpan","bigint"},
            {"Guid","uniqueidentifier"},
            {"Byte","tinyint"},
            {"Single","real"},
        };
        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
        {
            {"bigint","long"},
            {"binary","byte[]"},
            {"bit","bool"},
            {"char","string"},
            {"date","DateTime"},
            {"datetime","DateTime"},
            {"datetime2","DateTime"},
            {"datetimeoffset","DateTimeOffset"},
            {"decimal","decimal"},
            {"float","double"},
            {"image","byte[]"},
            {"int","int"},
            {"money","decimal"},
            {"nchar","string"},
            {"ntext","string"},
            {"numeric","decimal"},
            {"nvarchar","string"},
            {"real","float"},
            {"smalldatetime","DateTime"},
            {"smallint","short"},
            {"smallmoney","decimal"},
            {"text","string"},
            {"time","TimeSpan"},
            {"timestamp","byte[]"},
            {"tinyint","byte"},
            {"uniqueidentifier","Guid"},
            {"varbinary","byte[]"},
            {"varchar","string"}
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
                    item = DatabaseTypeToCsharpTypeMappings[databaseFieldInfo.ColumnDataType];
                }
                result.Add(item);
            }

            return result;
        }
    }
}