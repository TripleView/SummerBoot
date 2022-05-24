using System;
using System.Collections.Generic;
using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator.Dialect.Sqlite
{
    public class SqliteDatabaseFieldMapping : IDatabaseFieldMapping
    {
        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","TEXT"},
            {"Int32","INTEGER"},
            {"Int64","INTEGER"},
            {"DateTime","TEXT"},
            {"Decimal","TEXT"},
            {"Boolean","INTEGER"},
            {"Byte[]","BLOB"},
            {"Double","REAL"},
            {"Int16","INTEGER"},
            {"TimeSpan","TEXT"},
            {"Guid","TEXT"},
            {"Byte","INTEGER"},
            {"Single","REAL"},
        };
        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
        {
            {"TEXT","string"},
            {"BLOB","byte[]"},
            {"NUMERIC","byte[]"},
            {"INTEGER","long"},
            {"REAL","double"},
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

                if (item.HasText())
                {
                    result.Add(item);
                }
                else
                {
                    throw new NotSupportedException(databaseFieldInfo.ColumnDataType);
                }
               
            }

            return result;
        }
    }
}