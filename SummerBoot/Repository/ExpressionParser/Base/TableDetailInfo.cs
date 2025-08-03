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
    public class TableDetailInfo  
    {
        public string TableName { get; }

        public List<ColumnInfo> ColumnInfos { get; }
        /// <summary>
        /// Table Alias
        /// ±í±ðÃû
        /// </summary>
        public string TableAlias { get; set; }

        public string GetSqlSnippetByPropertyName(string propertyName)
        {
         
           var columnName= ColumnInfos.First(it => it.PropertyName == propertyName).ColumnName;
           return TableName + "." + columnName;
        }
        public TableDetailInfo(Type type)
        {

            if (type.IsGenericType)
            {
                throw new Exception("Generics are not supported and must be a base entity class");
            }
            ColumnInfos = new List<ColumnInfo>();

            TableName = DbQueryUtil.GetTableName(type);

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