using SummerBoot.Repository.ExpressionParser.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Base
{
    public class TableInfo<T> where T : class
    {
        public string TableName { get; }

        public List<ColumnInfo> ColumnInfos { get; }

        public string GetSqlSnippetByPropertyName(string propertyName)
        {
         
           var columnName= ColumnInfos.First(it => it.PropertyName == propertyName).ColumnName;
           return TableName + "." + columnName;
        }
        public TableInfo()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                throw new Exception("不支持泛型,必须为基础实体类");
            }
            ColumnInfos = new List<ColumnInfo>();

            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            TableName = tableAttribute?.Name ?? "a";

            var properties = type.GetProperties().Where(it => !it.GetCustomAttributes().OfType<NotMappedAttribute>().Any()).ToList();

            foreach (var propertyInfo in properties)
            {
                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();

                var ci = new ColumnInfo
                {
                    ColumnName = columnAttribute?.Name ?? propertyInfo.Name,
                    IsKey = keyAttribute != null,
                    Property = propertyInfo,
                    PropertyName = propertyInfo.Name,
                    IsNullable = propertyInfo.PropertyType.IsNullable()
                };

                ColumnInfos.Add(ci);
            }


        }
    }
}