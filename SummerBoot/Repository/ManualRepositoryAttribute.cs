using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ManualRepositoryAttribute : Attribute
    {
        /// <summary>
        /// 接口类型
        /// </summary>
        public Type InterfaceType { get; }

        public ManualRepositoryAttribute(Type interfaceType)
        {
            this.InterfaceType = interfaceType;
        }
    }
}