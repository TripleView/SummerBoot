﻿using System;
using System.Collections.Generic;
using System.Text;
using SummerBoot.Repository;

namespace SummerBoot.Test.Oracle
{
    public class OracleAutoRepositoryAttribute : AutoRepositoryAttribute
    {
    }

    public class OracleManualRepositoryAttribute : ManualRepositoryAttribute
    {
        public OracleManualRepositoryAttribute(Type type):base(type)
        {
            
        }
    }
    
}
