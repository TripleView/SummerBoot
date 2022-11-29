using System;
using System.Data;

namespace SummerBoot.Repository.Core
{
    /// <summary>
    /// 该接口负责自定义的将值转换为数据库参数和将数据库返回的值解析为目标值
    /// </summary>
    public interface ITypeHandler<T>
    {
        /// <summary>
        /// 将数据库返回值解析为特定的类型
        /// </summary>
        /// <param name="targetType">要解析的目标类型</param>
        /// <param name="value">要被解析的值</param>
        /// <returns></returns>
        T Parse( object value);
        /// <summary>
        /// 设置数据库命令里的参数
        /// </summary>
        /// <param name="parameter">数据库参数</param>
        /// <param name="value">具体参数值</param>
        void SetValue(IDbDataParameter parameter, T value);
    }
}

