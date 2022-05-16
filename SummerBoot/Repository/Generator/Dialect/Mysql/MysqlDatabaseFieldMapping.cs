using System;
using System.Collections.Generic;
using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator.Dialect
{
    public class MysqlDatabaseFieldMapping : IDatabaseFieldMapping
    {
        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","varchar"},
            {"Int32","int"},
            {"Int64","bigint"},
            {"DateTime","datetime"},
            {"Decimal","decimal"},
            {"Boolean","tinyint(1)"},
            {"Byte[]","binary"},
            {"Double","double"},
            {"Int16","smallint"},
            {"TimeSpan","time"},
            {"Guid","varbinary(16)"},
            {"Byte","tinyint unsigned"},
            {"Single","float"},
        };
        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
        {
            {"bigint","long"},
            {"bigint unsigned","ulong"},
            {"blob","byte[]"},
            {"date","DateTime"},
            {"datetime","DateTime"},
            {"double","double"},
            {"float","float"},
            {"int","int"},
            {"int unsigned","uint"},
            {"mediumblob","byte[]"},
            {"mediumtext","string"},
            {"longblob","byte[]"},
            {"longtext","string"},
            {"mediumint","int"},
            {"mediumint unsigned","uint"},
            {"smallint","short"},
            {"smallint unsigned","ushort"},
            {"text","string"},
            {"time","TimeSpan"},
            {"timestamp","DateTime"},
            {"tinyblob","byte[]"},
            {"tinyint","sbyte"},
            {"tinyint unsigned","byte"},
            {"tinytext","string"},
            {"varchar","string"},
            {"year","short"},
            {"json","string"},
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
                
                if (IsType(databaseFieldInfo, "binary")|| IsType(databaseFieldInfo, "varbinary"))
                {
                    item = "byte[]";
                }
                //特殊情况
                if (databaseFieldInfo.ColumnDataType == "varbinary(16)")
                {
                    item = "Guid";
                }
                if (IsType(databaseFieldInfo, "bit"))
                {
                    item = "ulong";
                }
                if (IsType(databaseFieldInfo, "tinyint(1)"))
                {
                    item = "bool";
                }
                if (IsType(databaseFieldInfo, "char")|| IsType(databaseFieldInfo, "varchar"))
                {
                    item = "string";
                }
                if (IsType(databaseFieldInfo, "decimal"))
                {
                    item = "decimal";
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