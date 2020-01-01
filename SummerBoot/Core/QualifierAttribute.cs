using System;

namespace SummerBoot.Core
{
    public class QualifierAttribute : Attribute
    {
        public string Name { get; }

        public QualifierAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("service Name must not be empty");
            }

            this.Name = name;
        }
    }
}