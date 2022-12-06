using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.Generator.Dialect.Sqlite
{
    public class SqliteDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbFactory dbFactory;
        private readonly DatabaseUnit databaseUnit;
        public SqliteDatabaseInfo(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
            this.databaseUnit = dbFactory.DatabaseUnit;
        }

        public GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo)
        {
            var tableName = tableInfo.Name;
            var fieldInfos = tableInfo.FieldInfos;

            var body = new StringBuilder();
            body.AppendLine($"CREATE TABLE \"{tableName}\" (");
           
            var hasKeyField = fieldInfos.Any(it => it.IsKey);
            
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var fieldInfo = fieldInfos[i];

                //行末尾是否有逗号
                var lastComma = "";
                if (i != fieldInfos.Count - 1)
                {
                    lastComma = ",";
                }
               

                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo)}{lastComma}");
                
            }

            body.AppendLine($")");


            var result = new GenerateDatabaseSqlResult()
            {
                Body = body.ToString(),
                Descriptions = new List<string>(),
                FieldModifySqls = new List<string>()
            };

            return result;
        }

        /// <summary>
        /// 通过字段信息生成生成表的sql
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        private string GetCreateFieldSqlByFieldInfo(DatabaseFieldInfoDto fieldInfo)
        {
            var identityString = fieldInfo.IsAutoCreate ? " AUTOINCREMENT" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var pk = fieldInfo.IsKey ? " PRIMARY KEY" : "";
            var columnDataType = fieldInfo.ColumnDataType;

            if (fieldInfo.SpecifiedColumnDataType.HasText())
            {
                columnDataType = fieldInfo.SpecifiedColumnDataType;
            }

            var result = $"\"{fieldInfo.ColumnName}\" {columnDataType} {nullableString}{pk}{identityString}";
            return result;
        }

        public string CreateTableDescription(string schema, string tableName, string description)
        {
            return "";
        }

        public string UpdateTableDescription(string schema, string tableName, string description)
        {
            return "";
        }

        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var sql = $"ALTER TABLE {tableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo)}";
            return sql;
        }

        public string CreateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            return "";
        }

        public DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName)
        {
            var dbConnection = dbFactory.GetDbConnection();

            var sql = @"SELECT sql FROM sqlite_master WHERE tbl_name = @tableName";
            var fieldInfos = new List<DatabaseFieldInfoDto>();

            var tableStruct = dbConnection.QueryFirstOrDefault<string>(databaseUnit,sql, new { tableName });
            if (tableStruct.HasText())
            {
                var tableStructArr = tableStruct.Split(Environment.NewLine).ToList();
                foreach (var line in tableStructArr)
                {
                    if (line.Contains("CREATE TABLE") || line == ")")
                    {
                        continue;
                    }
                    var fieldInfo = new DatabaseFieldInfoDto()
                    {
                        IsNullable = true,
                        IsKey = false,
                        IsAutoCreate = false,
                    };

                    var tempLine = line.Trim();

                    var lineArr = tempLine.Split(" ").ToList();
                    var hasFieldName = false;
                    foreach (var tempLinePart in lineArr)
                    {
                        var matchValue = Regex.Match(tempLinePart, "\"[^\"}]*\"");
                        if (matchValue.Success)
                        {
                            hasFieldName = true;
                            fieldInfo.ColumnName = matchValue.Value.Replace("\"", "");
                            continue;
                        }

                        if (hasFieldName)
                        {
                            fieldInfo.ColumnDataType = tempLinePart;
                            break;
                        }
                    }

                    if (tempLine.Contains("NOT NULL"))
                    {
                        fieldInfo.IsNullable = false;
                    }

                    if (tempLine.Contains("PRIMARY KEY"))
                    {
                        fieldInfo.IsKey = true;
                    }
                    if (tempLine.Contains("AUTOINCREMENT"))
                    {
                        fieldInfo.IsAutoCreate = true;
                    }
                    fieldInfos.Add(fieldInfo);
                }
            }

            var result = new DatabaseTableInfoDto()
            {
                Name = tableName,
                Description = "",
                FieldInfos = fieldInfos
            };

            return result;
        }

        public string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            throw new NotImplementedException();
        }

        public string BoxTableNameOrColumnName(string tableNameOrColumnName)
        {
            throw new NotImplementedException();
        }

        public string GetSchemaTableName(string schema, string tableName)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultSchema(string schema)
        {
            return "";
        }
    }
}