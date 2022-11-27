using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Test.IlGenerator
{

    public class GuidTypeHandler : ITypeHandler
    {
        public object Parse(Type targetType, object value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            throw new NotImplementedException();
        }
    }
}

