using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public class DbGenerator : IDbGenerator
    {
        private readonly IDatabaseFieldMapping databaseFieldMapping;
        private readonly IDbFactory dbFactory;
        private readonly IDatabaseInfo databaseInfo;

        public DbGenerator(IDatabaseFieldMapping databaseFieldMapping, IDbFactory dbFactory, IDatabaseInfo databaseInfo)
        {
            this.databaseFieldMapping = databaseFieldMapping;
            this.dbFactory = dbFactory;
            this.databaseInfo = databaseInfo;
        }

        public void ExecuteGenerateSql(GenerateDatabaseSqlResult generateDatabaseSqlResult)
        {
            var dbConnection = dbFactory.GetDbConnection();
            dbConnection.Execute(generateDatabaseSqlResult.Body);
            foreach (var description in generateDatabaseSqlResult.Descriptions)
            {
                dbConnection.Execute(description);
            }
        }

        public List<string> GenerateCsharpClass(List<string> tableNames, string classNameSpace)
        {
            //c#中可空的基本类型
            var csharpCanBeNullableType = new List<string>()
            {
                "int",
                "long",
                "float",
                "double",
                "decimal",
                "Guid",
                "short",
                "DateTime",
                "bool",
                "TimeSpan",
                "byte"
            };

            var result = new List<string>();
            foreach (var tableName in tableNames)
            {
                var tableInfo = databaseInfo.GetTableInfoByName(tableName);
                var columnInfos = tableInfo.FieldInfos;
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                sb.AppendLine("namespace " + classNameSpace);
                sb.AppendLine("{");
                if (tableInfo.Description.HasText())
                {
                    sb.AppendLine("   /// <summary>");
                    sb.AppendLine($"   ///{tableInfo.Description}");
                    sb.AppendLine("   /// </summary>");
                }
                sb.AppendLine($"   [Table(\"{tableName}\")]");
                sb.AppendLine($"   public class {tableName}");
                sb.AppendLine("   {");
                foreach (var columnInfo in columnInfos)
                {
                    if (columnInfo.Description.HasText())
                    {
                        sb.AppendLine("      /// <summary>");
                        sb.AppendLine($"      ///{columnInfo.Description}");
                        sb.AppendLine("      /// </summary>");
                    }
                    if (columnInfo.IsKey)
                    {
                        sb.AppendLine("      [Key]");
                    }
                    if (columnInfo.IsAutoCreate)
                    {
                        sb.AppendLine("      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                    }
                    sb.AppendLine($"      [Column(\"{columnInfo.ColumnName}\")]");
                    var fieldType = databaseFieldMapping.ConvertDatabaseTypeToCsharpType(new List<string>() { columnInfo.ColumnDataType }).First();
                    var nullablePart = (columnInfo.IsNullable && csharpCanBeNullableType.Any(it => it == fieldType)) ? "?" : "";

                    sb.AppendLine($"      public {fieldType}{nullablePart} {columnInfo.ColumnName} {{ get; set; }}");
                }
                sb.AppendLine("   }");
                sb.AppendLine("}");
                result.Add(sb.ToString());
            }

            return result;
        }

        public List<GenerateDatabaseSqlResult> GenerateSql(List<Type> types)
        {
            var result = new List<GenerateDatabaseSqlResult>();
            foreach (var type in types)
            {
                var tableAttribute = type.GetCustomAttribute<TableAttribute>();
                var tableDescriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
                var tableName = tableAttribute != null ? tableAttribute.Name : type.Name;
                var tableDescription = tableDescriptionAttribute?.Description ?? "";

                var propertys = type.GetProperties();
                var fieldInfos = new List<DatabaseFieldInfoDto>();
                foreach (var propertyInfo in propertys)
                {
                    var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var fieldName = columnAttribute != null ? columnAttribute.Name : propertyInfo.Name;
                    var requireAttribute = propertyInfo.GetCustomAttribute<RequiredAttribute>();
                    //数据库可空类型判断
                    var isNullable = propertyInfo.PropertyType.IsNullable();
                    var fieldTypeName = isNullable
                        ? propertyInfo.PropertyType.GetGenericArguments()[0]
                        : propertyInfo.PropertyType;

                    //如果是string类型，允许可空
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        isNullable = true;
                    }
                    //如果具有require注解，则默认不可空
                    if (requireAttribute != null)
                    {
                        isNullable = false;
                    }


                    var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();
                    var databaseGeneratedAttribute = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
                    var stringLengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
                    var decimalPrecisionAttribute = propertyInfo.GetCustomAttribute<DecimalPrecisionAttribute>();

                    var dbFieldTypeName = databaseFieldMapping.ConvertCsharpTypeToDatabaseType(new List<string>() { fieldTypeName.Name }).FirstOrDefault();
                    if (dbFieldTypeName.IsNullOrWhiteSpace())
                    {
                        throw new Exception($"can not convert {propertyInfo.PropertyType.Name} to database type");
                    }
                    //decimal精度问题
                    var precision = decimalPrecisionAttribute?.Precision?? 18;
                    var scale = decimalPrecisionAttribute?.Scale ?? 2;

                    var decimalPrecision = new DecimalPrecisionDto()
                    {
                        Precision = precision,
                        Scale = scale
                    };

                    var fieldInfo = new DatabaseFieldInfoDto()
                    {
                        ColumnName = fieldName,
                        ColumnDataType = dbFieldTypeName,
                        IsNullable = isNullable,
                        IsKey = keyAttribute != null,
                        IsAutoCreate = databaseGeneratedAttribute != null && databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity,
                        Description = descriptionAttribute != null ? descriptionAttribute.Description : "",
                        StringMaxLength = stringLengthAttribute?.MaximumLength,
                        DecimalPrecision= decimalPrecision
                    };

                    fieldInfos.Add(fieldInfo);
                }

                var item = databaseInfo.CreateTable(new DatabaseTableInfoDto()
                {
                    Description = tableDescription,
                    Name = tableName,
                    FieldInfos = fieldInfos
                });
                result.Add(item);
            }

            return result;
        }
    }
}