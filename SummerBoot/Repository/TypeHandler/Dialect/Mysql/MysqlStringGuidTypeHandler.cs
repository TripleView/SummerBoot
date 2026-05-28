using SummerBoot.Repository.Core;
using System;
using System.Data;

namespace SummerBoot.Repository.TypeHandler.Dialect.Mysql;

public class MysqlStringGuidTypeHandler : TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }

    public override Guid Parse(object value)
    {
        if (value is null)
        {
            return Guid.Empty;
        }
        else
        {
          return  Guid.Parse(value.ToString());
        }
        
        throw new NotImplementedException($"Error converting {value.GetType().Name} to System.Guid ");
    }
}