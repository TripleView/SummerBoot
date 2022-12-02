using System;
using System.Data;

namespace SummerBoot.Repository.Core
{
    public abstract class TypeHandler<T> : ITypeHandler
    {
        public abstract void SetValue(IDbDataParameter parameter, T value);

        public abstract T Parse(object value);

        object ITypeHandler.Parse(Type targetType, object value)
        {
            return this.Parse(value);
        }

        void ITypeHandler.SetValue(IDbDataParameter parameter, object value)
        {
            if (value is DBNull)
            {
                parameter.Value = value;
            }
            else
            {
                SetValue(parameter, (T)value);
            }
        }
    }
}
