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
    public class TableInfo
    {
        private Dictionary<string, ColumnInfo> propertyNameToColumnInfoMap = new Dictionary<string, ColumnInfo>();
        public string Name { get; set; }

        public string Schema { get; private set; }

        public List<ColumnInfo> Columns { get; }
        /// <summary>
        /// Table Alias
        /// ±í±ðÃû
        /// </summary>
        public string TableAlias { get; set; }

        public Type Type { get; }

        public TableInfo(Type type)
        {
            if (type.IsGenericType && !type.IsAnonymousType())
            {
                throw new Exception("Generics are not supported and must be a base entity class");
            }

            this.Type = type;
            Columns = new List<ColumnInfo>();
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            Name = tableAttribute?.Name?? type.Name ;
            Schema = tableAttribute?.Schema ?? "";

            var properties = type.GetProperties().Where(it => !it.GetCustomAttributes().OfType<NotMappedAttribute>().Any()).ToList();

            foreach (var propertyInfo in properties)
            {
                var ci = new ColumnInfo(propertyInfo);
                Columns.Add(ci);
            }

            propertyNameToColumnInfoMap = Columns.ToDictionary(x => x.PropertyName);
        }

        public ColumnInfo GetColumnInfo(PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            if (propertyNameToColumnInfoMap.TryGetValue(propertyName, out var columnInfo))
            {
                return columnInfo;
            }

            throw new Exception($"Attribute {propertyName} cannot be found in type {Type.FullName}");
        }
    }
}