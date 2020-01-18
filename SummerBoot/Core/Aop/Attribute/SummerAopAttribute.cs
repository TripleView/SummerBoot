using System;
using Castle.DynamicProxy;

namespace SummerBoot.Core.Aop.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SummerAopAttribute:System.Attribute
    {

    }
}