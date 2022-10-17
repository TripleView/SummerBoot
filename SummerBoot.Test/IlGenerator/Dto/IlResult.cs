using System;
using System.Linq;

namespace SummerBoot.Test.IlGenerator.Dto
{
    /// <summary>
    /// 值类型的结构体
    /// </summary>
    public struct IlValueTypeItem
    {
        public string Name { get; set; }
    }

    public class IlResult
    {
        public string Name2;
        public string Name { get; set; }
    }

    public class IlResultItem
    {
        public string Name { get; set; }
    }
}