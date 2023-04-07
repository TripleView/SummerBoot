using System;
using System.Data;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.TypeHandler.Dialect.Sqlite
{
    public class SqliteGuidTypeHandler : TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }

        public override Guid Parse(object value)
        {
            if (value is string stringValue)
            {
                return new Guid(stringValue);
            }

            throw new NotImplementedException($"Error converting {value.GetType().Name} to System.Guid ");
        }
    }
}