using SummerBoot.Core;
using SummerBoot.Repository.Generator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.Generator.Dialect.SqlServer
{
    public class SqlServerDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbFactory dbFactory;
        private readonly DatabaseUnit databaseUnit;
        public SqlServerDatabaseInfo(IDbFactory dbFactory)
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
                var tableDescriptionSql = CreateTableDescription(tableInfo.Schema,tableName, tableInfo.Description);
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
                    lastComma = hasKeyField? ",":"";
                }
                
                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo,false)}{lastComma}");
                if (fieldInfo.IsKey)
                {
                    keyField = fieldInfo.ColumnName;
                }

                //添加行注释
                if (fieldInfo.Description.HasText())
                {
                    var tableFieldDescription= CreateTableFieldDescription(tableInfo.Schema,tableName, fieldInfo);
                    databaseDescriptions.Add(tableFieldDescription);
                }
            }

            if (keyField.HasText())
            {
                body.AppendLine($"    CONSTRAINT PK_{tableName} PRIMARY KEY ({keyField})");
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
        private string GetCreateFieldSqlByFieldInfo(DatabaseFieldInfoDto fieldInfo, bool isAlter)
        {
            var identityString = fieldInfo.IsAutoCreate ? "IDENTITY(1,1)" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var columnDataType = fieldInfo.ColumnDataType;
            var primaryKeyString = fieldInfo.IsAutoCreate && fieldInfo.IsKey && isAlter ? "PRIMARY KEY " : "";
            var defaultString = fieldInfo.ColumnType.IsNumberType() && !fieldInfo.IsNullable  ? " DEFAULT 0 ":"";
            //string类型默认长度max，也可自定义
            if (fieldInfo.ColumnDataType == "nvarchar")
            {
                columnDataType = fieldInfo.StringMaxLength.HasValue && fieldInfo.StringMaxLength.Value != int.MaxValue
                    ? $"nvarchar({fieldInfo.StringMaxLength.Value})"
                    : $"nvarchar(max)";
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
            var result = $"{columnName} {columnDataType} {defaultString} {identityString} {primaryKeyString}{nullableString}";
            return result;
        }

        public string CreateTableDescription(string schema,string tableName, string description)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_addextendedproperty 'MS_Description', N'{description}', 'schema', N'{schema}', 'table', N'{tableName}'";
            return sql;
        }

        public string UpdateTableDescription(string schema,string tableName, string description)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_updateextendedproperty 'MS_Description', N'{description}', 'schema', N'{schema}', 'table', N'{tableName}'";
            return sql;
        }

        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo,true)}";
          return sql;
        }

        public string CreateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_addextendedproperty 'MS_Description', N'{fieldInfo.Description}', 'schema', N'{schema}', 'table', N'{tableName}', 'column', N'{fieldInfo.ColumnName}'";
            return sql;
        }

        public DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName)
        {
            schema = GetDefaultSchema(schema);
            var dbConnection = dbFactory.GetDbConnection();
            var sql = @"select c.name as columnName,t.name as columnDataType
                 ,convert(bit,c.IsNullable)  as isNullable
                 ,convert(bit,case when exists(select 1 from sysobjects where xtype='PK' and parent_obj=c.id and name in (
                     select name from sysindexes where indid in(
                         select indid from sysindexkeys where id = c.id and colid=c.colid))) then 1 else 0 end) 
                             as IsKey
                 ,convert(bit,COLUMNPROPERTY(c.id,c.name,'IsIdentity')) as isAutoCreate
                 ,c.Length as [占用字节] 
                 ,COLUMNPROPERTY(c.id,c.name,'PRECISION') as Precision
                 ,isnull(COLUMNPROPERTY(c.id,c.name,'Scale'),0) as Scale
                 ,ISNULL(CM.text,'') as [默认值]
                 ,isnull(ETP.value,'') AS Description
               from syscolumns c
               inner join systypes t on c.xusertype = t.xusertype 
               left join sys.extended_properties ETP on ETP.major_id = c.id and ETP.minor_id = c.colid and ETP.name ='MS_Description' 
               left join syscomments CM on c.cdefault=CM.id
               where c.id = (select t.object_id  from sys.tables t join sys.schemas s on t.schema_id=s.schema_id where s.name =@schemaName and t.name =@tableName) ";
            var fieldInfos = dbConnection.Query<DatabaseFieldInfoDto>(databaseUnit,sql, new { tableName, schemaName=schema }).ToList();

            var tableDescriptionSql = @"select etp.value from SYS.OBJECTS c
                    left join sys.extended_properties ETP on ETP.major_id = c.object_id   
                    where c.object_id =(select t.object_id  from sys.tables t join sys.schemas s on t.schema_id=s.schema_id where s.name =@schemaName and t.name =@tableName) and minor_id =0";

            var tableDescription = dbConnection.QueryFirstOrDefault<string>(databaseUnit, tableDescriptionSql, new { tableName, schemaName = schema });

            var result=new DatabaseTableInfoDto()
            {
                Name = tableName,
                Description = tableDescription,
                FieldInfos = fieldInfos
            };

            return result;
        }

        public string GetSchemaTableName(string schema, string tableName)
        {
            tableName = BoxTableNameOrColumnName(tableName);
            tableName = schema.HasText() ? schema + "." + tableName : tableName;
            return tableName;
        }

        public string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            //var schemaTableName = GetSchemaTableName(schema, tableName);
            //var sql =
            //    $"ALTER TABLE {schemaTableName} ADD CONSTRAINT {tableName}_PK PRIMARY KEY({fieldInfo.ColumnName})";

            //return sql;
            return "";
        }

        public string BoxTableNameOrColumnName(string tableNameOrColumnName)
        {
            return "[" + tableNameOrColumnName + "]";
        }

        public string GetDefaultSchema(string schema)
        {
            if (schema.HasText())
            {
                return schema;
            }
            var dbConnection = dbFactory.GetDbConnection();
            var result = dbConnection.QueryFirstOrDefault<string>(databaseUnit, "select SCHEMA_name()");
            return result;
        }
    }
}