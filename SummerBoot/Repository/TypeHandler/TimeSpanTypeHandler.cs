using System;
using System.Data;
using Dapper;

namespace SummerBoot.Repository.TypeHandler
{
    public class TimeSpanTypeHandler: SqlMapper.TypeHandler<TimeSpan>
    {
        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value;
        }

        public override TimeSpan Parse(object value)
        {
            if (value is TimeSpan timeSpanValue)
            {
                return timeSpanValue;
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to TimeSpan ");
        }
    }
}