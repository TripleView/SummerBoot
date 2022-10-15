using System;
using System.Linq;

namespace SummerBoot.Test.IlGenerator.Dto
{
    public class IlResult
    {
        public string Name { get; set; }

        public void Get123(Type tt)
        {
           var c= tt.GetConstructor(new Type[0]);
        }
    }

    public class IlResultItem
    {
        public string Name { get; set; }
    }
}