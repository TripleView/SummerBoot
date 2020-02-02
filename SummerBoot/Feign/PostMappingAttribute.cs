using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{

    public class PostMappingAttribute : HttpMappingAttribute
    {
        public PostMappingAttribute(string value):base(value)
        {
        }
    }
}
