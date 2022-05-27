using System;
using System.Data;
using Dapper;

namespace SummerBoot.Repository.TypeHandler.Dialect.Oracle
{
    /// <summary>
    /// Conversion between <see cref="Guid"/> and RAW(16) Oracle data type 
    /// </summary>
    public class GuidRaw16TypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToByteArray();
        }

        public override Guid Parse(object value)
        {
            if (value is byte[] bytearray)
            {
                return new Guid(bytearray);
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to System.Guid ");
        }
    }
}