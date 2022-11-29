using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Test.IlGenerator
{

    public class IntTypeHandler : ITypeHandler<int>
    {
        public int Parse(object value)
        {
            return (int)value;
        }

        public void SetValue(IDbDataParameter parameter, int value)
        {
            parameter.Value = value;
        }
    }
}

