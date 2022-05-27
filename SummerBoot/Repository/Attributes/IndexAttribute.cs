using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using SummerBoot.Core;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// 表示数据库索引的注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true,AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(params string[] propertyNames)
        {
            if (propertyNames.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(PropertyNames));
            }

            PropertyNames= propertyNames.ToList();
        }
        /// <summary>
        /// 索引的列名
        /// </summary>
        public IReadOnlyList<string> PropertyNames { get; }
        /// <summary>
        /// 是否为唯一索引
        /// </summary>
        public bool? IsUnique { get; set; }
        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name { get; set; }
    }
}