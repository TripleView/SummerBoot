using System.Reflection;

namespace SummerBoot.Repository.Core
{
    public class MemberCacheInfo
    {
        public string Name { get; set; }

        public string PropertyName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }
}