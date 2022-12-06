using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Repository.TypeHandler
{
    public class TimeSpanTypeHandler: TypeHandler<TimeSpan>
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