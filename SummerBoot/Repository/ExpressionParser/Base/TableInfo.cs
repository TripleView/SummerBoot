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
        public string Name { get; private set; }

        public string Schema { get; private set; }

        public List<ColumnInfo> Columns { get; }
        /// <summary>
        /// Table Alias
        /// ±í±ðÃû
        /// </summary>
        public string TableAlias { get; set; }

        public TableInfo(Type type)
        {
            if (type.IsGenericType)
            {
                throw new Exception("Generics are not supported and must be a base entity class");
            }
            Columns = new List<ColumnInfo>();
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            Name = tableAttribute == null ? type.Name : tableAttribute.Name;
            Schema = tableAttribute == null ? "" : tableAttribute.Schema;

            var properties = type.GetProperties().Where(it => !it.GetCustomAttributes().OfType<NotMappedAttribute>().Any()).ToList();

            foreach (var propertyInfo in properties)
            {
                var ci = new ColumnInfo(propertyInfo);
                Columns.Add(ci);
            }
        }
    }
}