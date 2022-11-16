using System.Reflection;

namespace SummerBoot.Core.Utils.Reflection
{
    public class MemberInfoCache
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 参数信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
        public int DataReaderIndex { get; set; }
    }
}