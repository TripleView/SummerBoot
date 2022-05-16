using SummerBoot.Repository.Generator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using SummerBoot.Core;

namespace SummerBoot.Repository.Generator.Dialect.SqlServer
{
    public class MysqlDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbFactory dbFactory;

        public MysqlDatabaseInfo(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo)
        {
            var tableName = tableInfo.Name;
            var fieldInfos = tableInfo.FieldInfos;

            var body = new StringBuilder();
            body.AppendLine($"CREATE TABLE {tableName} (");
            //主键
            var keyField = "";
            var hasKeyField = fieldInfos.Any(it => it.IsKey);
            //数据库注释
            var databaseDescriptions = new List<string>();
            if (tableInfo.Description.HasText())
            {
                var tableDescriptionSql = CreateTableDescription(tableInfo.Schema, tableName, tableInfo.Description);
                databaseDescriptions.Add(tableDescriptionSql);
            }

            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var fieldInfo = fieldInfos[i];

                //行末尾是否有逗号
                var lastComma = "";
                if (i != fieldInfos.Count - 1)
                {
                    lastComma = ",";
                }
                else
                {
                    lastComma = hasKeyField ? "," : "";
                }

                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo)}{lastComma}");
                if (fieldInfo.IsKey)
                {
                    keyField = fieldInfo.ColumnName;
                }

                //添加行注释
                if (fieldInfo.Description.HasText())
                {
                    var tableFieldDescription = CreateTableFieldDescription(tableInfo.Schema, tableName, fieldInfo);
                    databaseDescriptions.Add(tableFieldDescription);
                }
            }

            if (keyField.HasText())
            {
                body.AppendLine($"    PRIMARY KEY (`{keyField}`)");
            }

            body.AppendLine($")");


            var result = new GenerateDatabaseSqlResult()
            {
                Body = body.ToString(),
                Descriptions = databaseDescriptions,
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
            var identityString = fieldInfo.IsAutoCreate ? "AUTO_INCREMENT" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var columnDataType = fieldInfo.ColumnDataType;
            //string类型默认长度max，也可自定义
            if (fieldInfo.ColumnDataType == "varchar")
            {
                columnDataType = fieldInfo.StringMaxLength.HasValue && fieldInfo.StringMaxLength.Value != int.MaxValue
                    ? $"varchar({fieldInfo.StringMaxLength.Value})"
                    : $"text";
            }
            //自定义decimal精度类型
            if (fieldInfo.ColumnDataType == "decimal")
            {
                columnDataType =
                    $"decimal({fieldInfo.Precision},{fieldInfo.Scale})";
            }

            if (fieldInfo.SpecifiedColumnDataType.HasText())
            {
                columnDataType = fieldInfo.SpecifiedColumnDataType;
            }

            var result = $"`{fieldInfo.ColumnName}` {columnDataType} {nullableString} {identityString}";
            return result;
        }

        public string CreateTableDescription(string schema, string tableName, string description)
        {
            tableName = GetTableName(schema, tableName);
            var sql = $"ALTER TABLE {tableName} COMMENT = '{description}'";
            return sql;
        }

        private string GetTableName(string schema, string tableName)
        {
            schema = GetDefaultSchema(schema);
            tableName = schema.HasText() ? schema + "." + tableName : tableName;
            return tableName;
        }

        private string GetDefaultSchema(string schema)
        {
            return schema.GetValueOrDefault("");
        }

        public string UpdateTableDescription(string schema, string tableName, string description)
        {
            return CreateTableDescription(schema, tableName, description);
        }

        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            tableName = GetTableName(schema, tableName);
            var sql = $"ALTER TABLE {tableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo)}";
            return sql;
        }

        public string CreateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            tableName = GetTableName(schema, tableName);
            var sql = $"ALTER TABLE {tableName} MODIFY {GetCreateFieldSqlByFieldInfo(fieldInfo)} COMMENT '{fieldInfo.Description}'";
            return sql;
        }

        public DatabaseTableInfoDto GetTableInfoByName(string tableName)
        {
            var dbConnection = dbFactory.GetDbConnection();

            var sql = @"
                       SELECT
                        COLUMN_NAME AS columnName,
                        ORDINAL_POSITION AS '列的排列顺序',
                        COLUMN_DEFAULT AS '默认值',
                        CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END as isNullable,
                        DATA_TYPE AS columnDataType2,
                        CHARACTER_MAXIMUM_LENGTH AS '字符最大长度',
                        NUMERIC_PRECISION AS 'precision',
                        NUMERIC_SCALE AS 'Scale',
                        COLUMN_TYPE AS columnDataType,
                        COLUMN_COMMENT AS Description,
                        CASE WHEN COLUMN_KEY = 'PRI' THEN 1 ELSE 0 END as IsKey,
                        CASE WHEN EXTRA = 'auto_increment' THEN 1 ELSE 0 END AS isAutoCreate
                    FROM
                        information_schema.`COLUMNS`
                    WHERE
                        TABLE_NAME = @tableName
                    ORDER BY
                        TABLE_NAME,
                        ORDINAL_POSITION ";
            var fieldInfos = dbConnection.Query<DatabaseFieldInfoDto>(sql, new { tableName }).ToList();

            var tableDescriptionSql = @"SELECT 
                                        TABLE_COMMENT
                                        FROM information_schema.tables
                                        WHERE TABLE_NAME = @tableName ";

            var tableDescription = dbConnection.QueryFirstOrDefault<string>(tableDescriptionSql, new { tableName });

            var result = new DatabaseTableInfoDto()
            {
                Name = tableName,
                Description = tableDescription,
                FieldInfos = fieldInfos
            };

            return result;
        }
    }
}