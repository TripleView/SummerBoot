using System.Collections.Generic;
using System.Reflection;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryResult
    {
        public SqlExpression ExecuteSqlExpression { get; set; } 
        /// <summary>
        /// ִ�е�sql
        /// </summary>
        public string Sql
        {
            get => ExecuteSqlExpression.ToSql();
            set
            {

            }
        }
        /// <summary>
        /// ����������sql
        /// </summary>
        public string CountSql { get; set; }

        public DynamicParameters DynamicParameters { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public List<SqlParameter> SqlParameters { get; set; }
        /// <summary>
        /// �������ݿ���ȡID��sql
        /// </summary>
        public string LastInsertIdSql { get; set; }
        /// <summary>
        /// Id �ֶεķ���������Ϣ
        /// </summary>
        public PropertyInfo IdKeyPropertyInfo { get; set; }
        /// <summary>
        /// id�ֶε����ƣ���Щ���ݿ��Сд����
        /// </summary>
        public string IdName { get; set; }
        /// <summary>
        /// ���ֶε���Ϣ
        /// </summary>
        public List<DbQueryResultPropertyInfoMapping> PropertyInfoMappings { get; set; }
        /// <summary>
        /// Get dynamic parameters
        /// ��ȡ��̬����
        /// </summary>
        /// <returns></returns>
        public DynamicParameters GetDynamicParameters()
        {
            if (SqlParameters == null || SqlParameters.Count == 0)
            {
                return null;
            }

            var result = new DynamicParameters();
            foreach (var parameter in SqlParameters)
            {
                result.Add(parameter.ParameterName, parameter.Value, valueType: parameter.ParameterType);
            }

            return result;
        }
    }

    public class DbQueryResultPropertyInfoMapping
    {
        /// <summary>
        /// ����
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// ��Ӧ������
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}