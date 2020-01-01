using System;

namespace SummerBoot.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutowiredAttribute : Attribute
    {
        public bool Require { get; }
        public AutowiredAttribute(bool require = false)
        {
            this.Require = require;
        }
    }
}
