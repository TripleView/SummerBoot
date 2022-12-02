using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Test.IlGenerator
{

    public class IntTypeHandler : TypeHandler<int>
    {
        public override int Parse(object value)
        {
            return (int)value;
        }

        public override void SetValue(IDbDataParameter parameter, int value)
        {
            parameter.Value = value;
        }
    }
}

