﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public class GetMappingAttribute:HttpMappingAttribute
    {
        public GetMappingAttribute(string value):base(value)
        {
            
        }
    }
}
