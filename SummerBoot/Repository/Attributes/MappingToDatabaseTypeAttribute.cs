using System;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// 指定映射为特定的数据库类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class MappingToDatabaseTypeAttribute : Attribute
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType { get; set; }

        public MappingToDatabaseTypeAttribute(string databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}