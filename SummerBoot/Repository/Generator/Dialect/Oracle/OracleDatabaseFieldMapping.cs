using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.Generator.Dialect.Oracle
{
    public class OracleDatabaseFieldMapping : IDatabaseFieldMapping
    {
        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","NVARCHAR2"},
            {"Int32","NUMBER"},//int 10,0
            {"Int64","NUMBER"},//long 19,0
            {"DateTime","TIMESTAMP"},//7
            {"Decimal","NUMBER"},//18,2
            {"Boolean","NUMBER"},//1,0
            {"Byte[]","binary"},
            {"Double","BINARY_DOUBLE"},
            {"Int16","NUMBER"},//5,0
            {"TimeSpan","INTERVAL DAY(8) TO SECOND(7)"},//INTERVAL DAY (8) TO SECOND (7)
            {"Guid","RAW"},//16
            {"Byte","NUMBER"},//3,0
            {"Single","BINARY_FLOAT"},
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
            short a;
            var result = new List<string>();
            foreach (var type in csharpTypeList)
            {
                var item = csharpTypeToDatabaseTypeMappings[type];
                result.Add(item);
            }

            return result;
        }

        public List<string> ConvertDatabaseTypeToCsharpType(List<string> databaseTypeList)
        {
            var result = new List<string>();
            foreach (var type in databaseTypeList)
            {
                var item = DatabaseTypeToCsharpTypeMappings[type];
                result.Add(item);
            }

            return result;
        }
    }
}