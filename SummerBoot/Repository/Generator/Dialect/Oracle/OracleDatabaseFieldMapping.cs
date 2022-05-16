using System;
using System.Collections.Generic;
using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;

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

        public List<string> ConvertDatabaseTypeToCsharpType(List<DatabaseFieldInfoDto> databaseFieldInfoList)
        {
            var result = new List<string>();
            foreach (var fieldInfo in databaseFieldInfoList)
            {
                var item = "";
                //自定义NUMBER精度类型
                if (fieldInfo.ColumnDataType == "NUMBER")
                {
                    item = "decimal";
                    var precision = fieldInfo.Precision;
                    var scale = fieldInfo.Scale;

                    if (scale == 0)
                    {
                        if (precision >= 6 && precision <= 10)
                        {
                            item = "int";
                        }

                        if (precision >= 11 && precision <= 19)
                        {
                            item = "long";
                        }

                        if (precision == 1)
                        {
                            item = "bool";
                        }

                        if (precision == 5)
                        {
                            item = "short";

                        }

                        if (precision >= 2 && precision <= 4)
                        {
                            item = "byte";
                        }
                        if (precision > 19)
                        {

                            item = "decimal";
                        }
                    }
                    
                    if (precision > 0 && scale > 0)
                    {

                        item = "decimal";
                    }
                }
                //guid类型，默认16位
                if (fieldInfo.ColumnDataType == "RAW")
                {
                    item = $"Guid";
                }
                //datetime类型，默认7位
                if (fieldInfo.ColumnDataType == "TIMESTAMP(7)"|| fieldInfo.ColumnDataType == "DATE")
                {
                    item = $"DateTime";
                }
                //datetime类型，默认7位
                if (fieldInfo.ColumnDataType == "INTERVAL DAY(8) TO SECOND(7)")
                {
                    item = $"TimeSpan";
                }
                //double
                if (fieldInfo.ColumnDataType == "BINARY_DOUBLE")
                {
                    item = $"double";
                }
                //float
                if (fieldInfo.ColumnDataType == "BINARY_FLOAT")
                {
                    item = $"float";
                }
                
                if (fieldInfo.ColumnDataType.Contains("CHAR")|| fieldInfo.ColumnDataType == "JSON"
                                                             || fieldInfo.ColumnDataType == "CLOB" || fieldInfo.ColumnDataType == "NCLOB"
                                                             || fieldInfo.ColumnDataType == "XMLTYPE" || fieldInfo.ColumnDataType == "ROWID"
                                                             || fieldInfo.ColumnDataType == "UROWID" || fieldInfo.ColumnDataType == "LONG")
                {
                    item = $"string";
                }
                if ( fieldInfo.ColumnDataType == "BLOB"
                                                              || fieldInfo.ColumnDataType == "BFILE" || fieldInfo.ColumnDataType == "LONG RAW"
                                                             )
                {
                    item = $"byte[]";
                }

                result.Add(item);
            }

            return result;
        }
    }
}