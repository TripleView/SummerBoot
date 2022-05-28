﻿using System;
using System.Data;
using Dapper;

namespace SummerBoot.Repository.TypeHandler.Dialect.SqlServer
{
    public class SqlServerTimeSpanTypeHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value.Ticks;
        }

        public override TimeSpan Parse(object value)
        {
            if (value is long longTick)
            {
                return new TimeSpan(longTick);
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to TimeSpan ");
        }
    }
}