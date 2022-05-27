using Dapper;
using System.Data;

namespace SummerBoot.Repository.TypeHandler.Dialect.Oracle
{
    /// <summary>
    /// oracle bool类型和number类型的转换器
    /// </summary>
    public class BoolNumericTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override bool Parse(object value)
        {
            int.TryParse(value?.ToString(), out int result);
            return result > 0;
        }

        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }
    }

}