using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.ExpressionParser.Util;

namespace SummerBoot.Repository.ExpressionParser.Base
{
    public class ColumnInfo
    {
        public ColumnInfo(PropertyInfo propertyInfo)
        {
            var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();
            var ignoreWhenUpdateAttribute = propertyInfo.GetCustomAttribute<IgnoreWhenUpdateAttribute>();

            Name = DbQueryUtil.GetColumnName(propertyInfo);
            IsKey = keyAttribute != null;
            Property = propertyInfo;
            PropertyName = propertyInfo.Name;
            IsNullable = propertyInfo.PropertyType.IsNullable();
            IsIgnoreWhenUpdate = ignoreWhenUpdateAttribute != null;
            
            var databaseGenerated = Property.GetCustomAttribute<DatabaseGeneratedAttribute>();
            if (databaseGenerated != null)
            {
                var databaseGeneratedValue =
                    databaseGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
                if (databaseGeneratedValue)
                {
                    IsDatabaseGeneratedIdentity = true;
                }
            }
            
        }

        /// <summary>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsKey { get; private set; }
        /// <summary>
        /// </summary>
        public PropertyInfo Property { get; private set; }
        /// <summary>
        /// 判断是否可空
        /// </summary>
        public bool IsNullable { get; private set; }
        /// <summary>
        /// Ignore during update
        /// 更新中忽略
        /// </summary>
        public bool IsIgnoreWhenUpdate { get; private set; }


        /// <summary>
        /// Determine if it is a database auto-increment.
        /// 判断是否为数据库自增
        /// </summary>
        public bool IsDatabaseGeneratedIdentity { get;private set; }
    }
}