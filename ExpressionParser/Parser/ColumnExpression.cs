using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionParser.Util;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// 列表达式
    /// </summary>
    public class ColumnExpression : DbBaseExpression
    {

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index) : base((ExpressionType)DbExpressionType.Column, type)
        {
            TableAlias = tableAlias;
            Index = index;
            this.MemberInfo = memberInfo;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, object value, string functionName) : this(type, tableAlias, memberInfo, index)
        {
            this.Value = value;
            this.FunctionName = functionName;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, object value) : this(type, tableAlias, memberInfo, index)
        {
            this.Value = value;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, string columnAlias) : this(type, tableAlias, memberInfo, index)
        {
            this.ColumnAlias = columnAlias;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, string columnAlias, string functionName) : this(type, tableAlias, memberInfo, index)
        {
            this.ColumnAlias = columnAlias;
            this.FunctionName = functionName;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, string columnAlias, string functionName,object value) : this(type, tableAlias, memberInfo, index, columnAlias, functionName)
        {
            this.ColumnAlias = columnAlias;
            this.FunctionName = functionName;
            this.Value = value;
        }

        /// <summary>
        /// 方法元信息
        /// </summary>
        public MemberInfo MemberInfo { get; }
        /// <summary>
        /// 包围列的函数，len
        /// </summary>
        public string FunctionName { get; set; }
        #region 属性
        /// <summary>
        /// 固定值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 判断是否为主键
        /// </summary>
        public bool IsKey
        {
            get
            {
                if (MemberInfo == null)
                {
                    return false;
                }
                var keyAttribute = MemberInfo.GetCustomAttribute<KeyAttribute>();
                return keyAttribute != null;
            }
        }

        /// <summary>
        /// 判断是否为可空类型
        /// </summary>
        public bool IsNullable => this.MemberInfo.IsNullable();

        /// <summary>
        /// 表的别名
        /// </summary>
        public string TableAlias { get; set; }
        /// <summary>
        /// 列的别名
        /// </summary>
        public string ColumnAlias { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName => DbQueryUtil.GetColumnName(MemberInfo);

        /// <summary>
        /// 排序
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 深度克隆本身
        /// </summary>
        /// <returns></returns>
        public ColumnExpression DeepClone()
        {
            var newColumnExpression =
                new ColumnExpression(this.Type, this.TableAlias, this.MemberInfo, this.Index, this.Value)
                {
                    ColumnAlias = this.ColumnAlias,
                    FunctionName = this.FunctionName
                };
            return newColumnExpression;
        }
        #endregion
    }
}