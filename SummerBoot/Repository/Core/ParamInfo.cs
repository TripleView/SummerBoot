using System.Data;
using System;

namespace SummerBoot.Repository.Core
{
    /// <summary>
    /// 参数信息
    /// </summary>
    public class ParamInfo
    {
        /// <summary>
        /// 实际关联的数据库参数
        /// </summary>
        public IDbDataParameter AssociatedActualParameters { get; set; }
       
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 值的类型
        /// </summary>
        public Type ValueType { get; set; }
        /// <summary>
        /// 参数方向
        /// </summary>
        public ParameterDirection ParameterDirection { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public DbType? DbType { get; set; }
        /// <summary>
        /// 参数大小
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// 小数点前精度
        /// </summary>
        public byte? Precision { get; set; }
        /// <summary>
        /// 小数点后精度
        /// </summary>
        public byte? Scale { get; set; }
    }
}
