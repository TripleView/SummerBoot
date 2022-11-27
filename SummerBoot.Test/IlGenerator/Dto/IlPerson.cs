using System;

namespace SummerBoot.Test.IlGenerator.Dto
{

    public class IlPerson
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class IlPersonSubClass : IlPerson
    {
        public string Address { get; set; }
    }
}