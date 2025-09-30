using SummerBoot.Repository.ExpressionParser.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;

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
                var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();
                var ignoreWhenUpdateAttribute = propertyInfo.GetCustomAttribute<IgnoreWhenUpdateAttribute>();

                var ci = new ColumnInfo
                {
                    ColumnName = DbQueryUtil.GetColumnName(propertyInfo),
                    IsKey = keyAttribute != null,
                    Property = propertyInfo,
                    PropertyName = propertyInfo.Name,
                    IsNullable = propertyInfo.PropertyType.IsNullable(),
                    IsIgnoreWhenUpdate = ignoreWhenUpdateAttribute != null
                };

                ColumnInfos.Add(ci);
            }


        }
    }
}