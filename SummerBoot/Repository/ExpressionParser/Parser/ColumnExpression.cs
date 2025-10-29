using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.ExpressionParser.Util;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// �б��ʽ
    /// </summary>
     public class ColumnExpression : DbBaseExpression
    {

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index) : base((ExpressionType)DbExpressionType.Column, type)
        {
            TableAlias = tableAlias;
            Index = index;
            this.MemberInfo = memberInfo;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index,Type valueType) : base((ExpressionType)DbExpressionType.Column, type)
        {
            TableAlias = tableAlias;
            Index = index;
            this.MemberInfo = memberInfo;
            this.ValueType=valueType;
        }

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, object value) : this(type, tableAlias, memberInfo, index)
        {
            this.Value = value;
        }

      

        public ColumnExpression(Type type, string tableAlias, MemberInfo memberInfo, int index, string columnAlias, string functionName) : this(type, tableAlias, memberInfo, index)
        {
            this.ColumnAlias = columnAlias;
            this.FunctionName = functionName;
        }

        public TableExpression Table { get; set; }
        /// <summary>
        /// ����Ԫ��Ϣ
        /// </summary>
        public MemberInfo MemberInfo { get; }
        /// <summary>
        /// ��Χ�еĺ�����len
        /// </summary>
        public string FunctionName { get; set; }
        #region ����
        /// <summary>
        /// �̶�ֵ
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// ֵ������
        /// </summary>
        public Type ValueType { get; set; }
        /// <summary>
        /// �ж��Ƿ�Ϊ����
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
        /// Determine whether to ignore during update
        /// �ж�updateʱ�Ƿ����
        /// </summary>
        public bool IsIgnoreWhenUpdate
        {
            get
            {
                if (MemberInfo == null)
                {
                    return false;
                }
                var ignoreWhenUpdateAttribute = MemberInfo.GetCustomAttribute<IgnoreWhenUpdateAttribute>();
                return ignoreWhenUpdateAttribute != null;
            }
        }

        /// <summary>
        /// �ж��Ƿ�Ϊ���ݿ�����
        /// </summary>
        public bool IsDatabaseGeneratedIdentity
        {
            get
            {
                if (MemberInfo == null)
                {
                    return false;
                }
                var databaseGenerated = MemberInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (databaseGenerated != null)
                {
                    var databaseGeneratedValue =
                        databaseGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
                    if (databaseGeneratedValue)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        /// <summary>
        /// �ж��Ƿ�Ϊ�ɿ�����
        /// </summary>
        public bool IsNullable => this.MemberInfo.IsNullable();

        /// <summary>
        /// ��ı���
        /// </summary>
        public string TableAlias { get; set; }
        /// <summary>
        /// �еı���
        /// </summary>
        public string ColumnAlias { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string ColumnName => DbQueryUtil.GetColumnName(MemberInfo);

        /// <summary>
        /// ����
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// ��ȿ�¡����
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