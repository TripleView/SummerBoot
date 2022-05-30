using System;
using System.Data;
using Dapper;

namespace SummerBoot.Repository.TypeHandler
{
    /// <summary>
    /// Conversion between <see cref="Guid"/> and byte array data type 
    /// </summary>
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
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