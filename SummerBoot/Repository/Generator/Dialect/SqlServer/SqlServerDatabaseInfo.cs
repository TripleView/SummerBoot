using SummerBoot.Repository.Generator.Dto;
using System.Collections.Generic;
using System.Linq;
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

        public string CreateTable(string tableName, List<DatabaseFieldInfoDto> fieldInfos)
        {
            var result = new StringBuilder();
            result.AppendLine($"CREATE TABLE {tableName} (");
            //主键
            var keyField = "";
            var hasKeyField = fieldInfos.Any(it => it.IsKey);
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var fieldInfo = fieldInfos[i];
                var identityString = fieldInfo.IsAutoCreate ? "IDENTITY(0,1)" : "";
                var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
                var columnDataType = fieldInfo.ColumnDataType;
                
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

                result.AppendLine($"    [{fieldInfo.ColumnName}] {columnDataType} {identityString} {nullableString}{lastComma}");
                if (fieldInfo.IsKey)
                {
                    keyField = fieldInfo.ColumnName;
                }
            }

            if (keyField.HasText())
            {
                result.AppendLine($"    CONSTRAINT PK_{tableName} PRIMARY KEY ({keyField})");
            }
            
            result.AppendLine($")");
            return result.ToString();
        }

        public List<DatabaseFieldInfoDto> GetTableInfoByName(string tableName)
        {
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
            var result = dbFactory.GetDbConnection().Query<DatabaseFieldInfoDto>(sql, new { tableName }).ToList();
            return result;
        }
    }
}