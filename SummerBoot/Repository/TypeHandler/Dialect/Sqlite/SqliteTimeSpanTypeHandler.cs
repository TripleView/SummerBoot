using System;
using System.Data;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.TypeHandler.Dialect.Sqlite
{
    public class SqliteTimeSpanTypeHandler : TypeHandler<TimeSpan>
    {
        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value.Ticks.ToString();
        }

        public override TimeSpan Parse(object value)
        {
            if (value is string stringValue && long.TryParse(stringValue,out var longTick))
            {
                return new TimeSpan(longTick);
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to TimeSpan ");
        }
    }
}