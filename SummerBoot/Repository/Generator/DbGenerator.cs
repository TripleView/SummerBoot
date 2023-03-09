using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public class DbGenerator : IDbGenerator
    {
        private readonly IDatabaseFieldMapping databaseFieldMapping;
        private readonly IDbFactory dbFactory;
        private readonly IDatabaseInfo databaseInfo;
        private readonly DatabaseUnit databaseUnit;

        public DbGenerator(IDatabaseFieldMapping databaseFieldMapping, IDbFactory dbFactory, IDatabaseInfo databaseInfo)
        {
            this.databaseFieldMapping = databaseFieldMapping;
            this.dbFactory = dbFactory;
            this.databaseUnit = dbFactory.DatabaseUnit;
            this.databaseInfo = databaseInfo;
        }

        public void ExecuteGenerateSql(GenerateDatabaseSqlResult generateDatabaseSqlResult)
        {
            var dbConnection = dbFactory.GetDbConnection();
            if (generateDatabaseSqlResult.Body.HasText())
            {
                dbConnection.Execute(databaseUnit, generateDatabaseSqlResult.Body);
            }

            foreach (var fieldModifySql in generateDatabaseSqlResult.FieldModifySqls)
            {
                dbConnection.Execute(databaseUnit, fieldModifySql);
            }

            foreach (var description in generateDatabaseSqlResult.Descriptions)
            {
                dbConnection.Execute(databaseUnit, description);
            }

            dbConnection.Close();
        }

        public List<string> GenerateCsharpClass( List<string> tableNames,  string classNameSpace, string namesapce = "")
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
                var tableInfo = databaseInfo.GetTableInfoByName(namesapce, tableName);
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
                var cshapClassName = tableName.Length > 0 && char.IsLower(tableName[0]) ? char.ToUpper(tableName[0]) + tableName.Substring(1) : tableName;

                sb.AppendLine($"   public class {cshapClassName}");
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
                    var fieldType = databaseFieldMapping.ConvertDatabaseTypeToCsharpType(new List<DatabaseFieldInfoDto>() { columnInfo }).First();
                    var nullablePart = (columnInfo.IsNullable && csharpCanBeNullableType.Any(it => it == fieldType)) ? "?" : "";
                    //为了符合c#命名规范，重新计算列名
                    var columnName = columnInfo.ColumnName;
                    if (columnName.Length > 0)
                    {
                        if (columnName.Contains(" "))
                        {
                            var tempColumnNameArr = new List<string>();
                            var columnNameParts = columnName.Split(" ").ToList();
                            foreach (var columnNamePart in columnNameParts)
                            {
                                if (columnNamePart.HasText())
                                {
                                    var tempColumnNamePart = columnNamePart;
                                    if (char.IsLetter(columnNamePart[0]) && char.IsLower(columnNamePart[0]))
                                    {
                                        tempColumnNamePart = char.ToUpper(columnNamePart[0])
                                                             + columnNamePart.Substring(1, columnNamePart.Length - 1);
                                    }
                                    tempColumnNameArr.Add(tempColumnNamePart);
                                }
                            }

                            columnName = string.Join(string.Empty, tempColumnNameArr);
                        }
                        else
                        {
                            if (columnName.HasText())
                            {
                                if (char.IsLetter(columnName[0]) && char.IsLower(columnName[0]))
                                {
                                    columnName = char.ToUpper(columnName[0])
                                                 + columnName.Substring(1, columnName.Length - 1);
                                }
                            }
                        }
                    }

                    sb.AppendLine($"      public {fieldType}{nullablePart} {columnName} {{ get; set; }}");
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
                var schema = tableAttribute?.Schema;
                schema = databaseInfo.GetDefaultSchema(schema);
                var tableDescription = tableDescriptionAttribute?.Description ?? "";
                var propertys = type.GetProperties();
                var fieldInfos = new List<DatabaseFieldInfoDto>();
                foreach (var propertyInfo in propertys)
                {
                    var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var fieldName = columnAttribute != null ? columnAttribute.Name : propertyInfo.Name;
                    var requireAttribute = propertyInfo.GetCustomAttribute<RequiredAttribute>();
                    //忽略字段
                    var notMappedAttribute = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                    if (notMappedAttribute != null)
                    {
                        continue;
                    }
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
                    //如果是枚举类型，统一转为int
                    if (propertyInfo.PropertyType.IsEnum || (propertyInfo.PropertyType.IsNullable() && propertyInfo.PropertyType.GetUnderlyingType().IsEnum))
                    {
                        fieldTypeName = typeof(int);
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
                    var precision = decimalPrecisionAttribute?.Precision ?? 18;
                    var scale = decimalPrecisionAttribute?.Scale ?? 2;
                    if (databaseUnit.ColumnNameMapping != null)
                    {
                        fieldName = databaseUnit.ColumnNameMapping(fieldName);
                    }
                    var fieldInfo = new DatabaseFieldInfoDto()
                    {
                        ColumnName = fieldName,
                        ColumnDataType = dbFieldTypeName,
                        SpecifiedColumnDataType = columnAttribute?.TypeName,
                        IsNullable = isNullable,
                        IsKey = keyAttribute != null,
                        IsAutoCreate = databaseGeneratedAttribute != null && databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity,
                        Description = descriptionAttribute != null ? descriptionAttribute.Description : "",
                        StringMaxLength = stringLengthAttribute?.MaximumLength,
                        Precision = precision,
                        Scale = scale,
                        ColumnType = propertyInfo.PropertyType
                    };

                    fieldInfos.Add(fieldInfo);
                }

               
                if (databaseUnit.TableNameMapping != null)
                {
                    tableName = databaseUnit.TableNameMapping(tableName);
                }
                //判断数据库中是否已经有这张表，如果有，则比较两张表的结构
                var dbTableInfo = databaseInfo.GetTableInfoByName(schema, tableName);
                //如果存在这张表
                if (dbTableInfo.FieldInfos.Any())
                {
                    var item = new GenerateDatabaseSqlResult()
                    {
                        Descriptions = new List<string>(),
                        FieldModifySqls = new List<string>()
                    };

                    if (dbTableInfo.Description.IsNullOrEmpty() && tableDescription.HasText())
                    {
                        var newTableDescriptionSql = databaseInfo.CreateTableDescription(schema, tableName, tableDescription);
                        if (newTableDescriptionSql.HasText())
                        {
                            item.Descriptions.Add(newTableDescriptionSql);
                        }

                    }
                    foreach (var fieldInfo in fieldInfos)
                    {
                        var dbFieldInfo =
                            dbTableInfo.FieldInfos.FirstOrDefault(it => it.ColumnName == fieldInfo.ColumnName);
                        if (dbFieldInfo == null)
                        {
                            var createFieldSql = databaseInfo.CreateTableField(schema, tableName, fieldInfo);
                            item.FieldModifySqls.Add(createFieldSql);
                            if (fieldInfo.Description.HasText())
                            {
                                var createFieldDescriptionSql = databaseInfo.CreateTableFieldDescription(schema, tableName, fieldInfo);
                                if (createFieldDescriptionSql.HasText())
                                {
                                    item.Descriptions.Add(createFieldDescriptionSql);
                                }
                            }
                            //添加约束
                            if (fieldInfo.IsKey)
                            {
                                var createPrimaryKeySql = databaseInfo.CreatePrimaryKey(schema, tableName, fieldInfo);

                                if (createPrimaryKeySql.HasText())
                                {
                                    item.FieldModifySqls.Add(createPrimaryKeySql);
                                }
                            }
                        }
                    }
                    result.Add(item);
                }
                else
                //不存在这张表则新建
                {
                    var item = databaseInfo.CreateTable(new DatabaseTableInfoDto()
                    {
                        Description = tableDescription,
                        Name = tableName,
                        FieldInfos = fieldInfos,
                        Schema = schema
                    });
                    result.Add(item);
                }
            }

            return result;
        }
    }
}