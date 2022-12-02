using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Test.IlGenerator
{

    public class GuidTypeHandler : TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value is string str)
            {
                return Guid.Parse(str);
            }
            return Guid.NewGuid();
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value;
        }
    }
}

