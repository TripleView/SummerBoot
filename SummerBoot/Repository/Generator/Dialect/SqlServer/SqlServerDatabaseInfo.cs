using SummerBoot.Repository.Generator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using SummerBoot.Core;

namespace SummerBoot.Repository.Generator.Dialect.SqlServer
{
    public class SqlServerDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbFactory dbFactory;

        public SqlServerDatabaseInfo(IDbFactory dbFactory)
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
                
                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo)}{lastComma}");
                if (fieldInfo.IsKey)
                {
                    keyField = fieldInfo.ColumnName;
                }

                //添加行注释
                if (fieldInfo.Description.HasText())
                {
                    var tableFieldDescription= CreateTableFieldDescription(tableInfo.Schema,tableName, fieldInfo.ColumnName, fieldInfo.Description);
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
        private string GetCreateFieldSqlByFieldInfo(DatabaseFieldInfoDto fieldInfo)
        {
            var identityString = fieldInfo.IsAutoCreate ? "IDENTITY(1,1)" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var columnDataType = fieldInfo.ColumnDataType;
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
                    $"decimal({fieldInfo.DecimalPrecision.Precision},{fieldInfo.DecimalPrecision.Scale})";
            }

            if (fieldInfo.SpecifiedColumnDataType.HasText())
            {
                columnDataType = fieldInfo.SpecifiedColumnDataType;
            }

            var result = $"[{fieldInfo.ColumnName}] {columnDataType} {identityString} {nullableString}";
            return result;
        }

        public string CreateTableDescription(string schema,string tableName, string description)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_addextendedproperty 'MS_Description', N'{description}', 'schema', N'{schema}', 'table', N'{tableName}'";
            return sql;
        }

        private string GetDefaultSchema(string schema)
        {
            return schema.GetValueOrDefault("dbo");
        }

        public string UpdateTableDescription(string schema,string tableName, string description)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_updateextendedproperty 'MS_Description', N'{description}', 'schema', N'{schema}', 'table', N'{tableName}'";
            return sql;
        }

        public string CreateTableField(string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var sql = $"ALTER TABLE {tableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo)}";
          return sql;
        }

        public string CreateTableFieldDescription(string schema,string tableName, string columnName, string description)
        {
            schema = GetDefaultSchema(schema);
            var sql =
                $"EXEC sp_addextendedproperty 'MS_Description', N'{description}', 'schema', N'{schema}', 'table', N'{tableName}', 'column', N'{columnName}'";
            return sql;
        }

        public DatabaseTableInfoDto GetTableInfoByName(string tableName)
        {
            var dbConnection = dbFactory.GetDbConnection();
            var sql = @"select c.name as columnName,t.name as columnDataType
                 ,convert(bit,c.IsNullable)  as isNullable
                 ,convert(bit,case when exists(select 1 from sysobjects where xtype='PK' and parent_obj=c.id and name in (
                     select name from sysindexes where indid in(
                         select indid from sysindexkeys where id = c.id and colid=c.colid))) then 1 else 0 end) 
                             as IsKey
                 ,convert(bit,COLUMNPROPERTY(c.id,c.name,'IsIdentity')) as isAutoCreate
                 ,c.Length as [占用字节] 
                 ,COLUMNPROPERTY(c.id,c.name,'PRECISION') as [长度]
                 ,isnull(COLUMNPROPERTY(c.id,c.name,'Scale'),0) as [小数位数]
                 ,ISNULL(CM.text,'') as [默认值]
                 ,isnull(ETP.value,'') AS Description
               from syscolumns c
               inner join systypes t on c.xusertype = t.xusertype 
               left join sys.extended_properties ETP on ETP.major_id = c.id and ETP.minor_id = c.colid and ETP.name ='MS_Description' 
               left join syscomments CM on c.cdefault=CM.id
               where c.id = object_id(@tableName) ";
            var fieldInfos = dbConnection.Query<DatabaseFieldInfoDto>(sql, new { tableName }).ToList();

            var tableDescriptionSql = @"select etp.value from SYS.OBJECTS c
                    left join sys.extended_properties ETP on ETP.major_id = c.object_id   
                    where c.name =@tableName and minor_id =0";

            var tableDescription = dbConnection.QueryFirstOrDefault<string>(tableDescriptionSql, new { tableName });

            var result=new DatabaseTableInfoDto()
            {
                Name = tableName,
                Description = tableDescription,
                FieldInfos = fieldInfos
            };

            return result;
        }
    }
}