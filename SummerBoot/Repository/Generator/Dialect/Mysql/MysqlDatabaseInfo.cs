using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.Generator.Dialect.SqlServer
{
    public class MysqlDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbFactory dbFactory;
        private readonly DatabaseUnit databaseUnit;
        public MysqlDatabaseInfo(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
            this.databaseUnit = dbFactory.DatabaseUnit;
        }

        public GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo)
        {
            var tableName = tableInfo.Name;
            var fieldInfos = tableInfo.FieldInfos;
            var schemaTableName = GetSchemaTableName(tableInfo.Schema, tableName);
            var body = new StringBuilder();
            body.AppendLine($"CREATE TABLE {schemaTableName} (");
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

                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo, false)}{lastComma}");
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
        /// <param name="isAlert"></param>
        /// <returns></returns>
        private string GetCreateFieldSqlByFieldInfo(DatabaseFieldInfoDto fieldInfo, bool isAlter)
        {
            var identityString = fieldInfo.IsAutoCreate && fieldInfo.IsKey ? "AUTO_INCREMENT" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var primaryKeyString = fieldInfo.IsAutoCreate && fieldInfo.IsKey && isAlter ? "PRIMARY KEY " : "";
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

            var columnName = BoxTableNameOrColumnName(fieldInfo.ColumnName);
            var result = $"{columnName} {columnDataType} {nullableString} {primaryKeyString}{identityString}";
            return result;
        }

        public string CreateTableDescription(string schema, string tableName, string description)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} COMMENT = '{description}'";
            return sql;
        }


        public string UpdateTableDescription(string schema, string tableName, string description)
        {
            return CreateTableDescription(schema, tableName, description);
        }

        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo, true)}";
            return sql;
        }

        public string CreateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} MODIFY {GetCreateFieldSqlByFieldInfo(fieldInfo, true)} COMMENT '{fieldInfo.Description}'";
            return sql;
        }

        public DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName)
        {
            var dbConnection = dbFactory.GetDbConnection();
            schema = GetDefaultSchema(schema);
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
                        TABLE_SCHEMA =@schemaName and
                        TABLE_NAME = @tableName
                    ORDER BY
                        TABLE_NAME,
                        ORDINAL_POSITION ";
            var fieldInfos = dbConnection.Query<DatabaseFieldInfoDto>(databaseUnit,sql, new { tableName, schemaName=schema }).ToList();

            var tableDescriptionSql = @"SELECT 
                                        TABLE_COMMENT
                                        FROM information_schema.tables
                                        WHERE TABLE_SCHEMA =@schemaName and TABLE_NAME = @tableName ";

            var tableDescription = dbConnection.QueryFirstOrDefault<string>(databaseUnit, tableDescriptionSql, new { tableName, schemaName = schema });

            var result = new DatabaseTableInfoDto()
            {
                Name = tableName,
                Description = tableDescription,
                FieldInfos = fieldInfos
            };

            return result;
        }

        public string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            //var schemaTableName = GetSchemaTableName(schema, tableName);
            //var sql =
            //    $"ALTER TABLE {schemaTableName} ADD CONSTRAINT {tableName}_PK PRIMARY KEY({BoxTableNameOrColumnName(fieldInfo.ColumnName)})";

            //return sql;
            return "";
        }

        public string BoxTableNameOrColumnName(string tableNameOrColumnName)
        {
            return "`" + tableNameOrColumnName + "`";
        }

        public string GetSchemaTableName(string schema, string tableName)
        {
            tableName = BoxTableNameOrColumnName(tableName);
            tableName = schema.HasText() ? schema + "." + tableName : tableName;
            return tableName;
        }

        public string GetDefaultSchema(string schema)
        {
            if (schema.HasText())
            {
                return schema;
            }
            var dbConnection = dbFactory.GetDbConnection();
            return dbConnection.Database;
        }
    }
}