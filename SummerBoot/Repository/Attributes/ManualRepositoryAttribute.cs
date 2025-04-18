using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepositoryAttribute:Attribute
    {
        public ManualRepositoryAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
        public Type InterfaceType { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository1Attribute : ManualRepositoryAttribute
    {
        public ManualRepository1Attribute(Type interfaceType):base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository2Attribute : ManualRepositoryAttribute
    {
        public ManualRepository2Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository3Attribute : ManualRepositoryAttribute
    {
        public ManualRepository3Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }

    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository4Attribute : ManualRepositoryAttribute
    {
        public ManualRepository4Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository5Attribute : ManualRepositoryAttribute
    {
        public ManualRepository5Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository6Attribute : ManualRepositoryAttribute
    {
        public ManualRepository6Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository7Attribute : ManualRepositoryAttribute
    {
        public ManualRepository7Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository8Attribute : ManualRepositoryAttribute
    {
        public ManualRepository8Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManualRepository9Attribute : ManualRepositoryAttribute
    {
        public ManualRepository9Attribute(Type interfaceType) : base(interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}