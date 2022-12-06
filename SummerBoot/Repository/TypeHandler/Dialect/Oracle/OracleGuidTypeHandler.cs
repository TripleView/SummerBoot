using System;
using System.Data;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.TypeHandler.Dialect.Oracle
{
    public class OracleGuidTypeHandler : TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.DbType = DbType.Binary;
            parameter.Value = value.ToByteArray();
        }

        public override Guid Parse(object value)
        {
            if (value is Guid guidValue)
            {
                return guidValue;
            }
            if (value is byte[] bytearray)
            {
                return new Guid(bytearray);
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to System.Guid ");
        }
    }
}