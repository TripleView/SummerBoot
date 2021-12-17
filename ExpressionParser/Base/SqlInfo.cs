using System;
using System.Collections.Generic;

namespace ExpressionParser.Base
{
    /// <summary>
    /// sql信息体
    /// </summary>
    public class SqlInfo
    {
        private static readonly List<string> all26Words = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private string sql;
        private string whereSql;
        private readonly Dictionary<string, object> parameters;

        //private readonly 
        private int paramterIndex => parameters.Count;

        private Queue<string> allTableAlisa;
        /// <summary>
        /// 设置选择的字段
        /// </summary>
        public List<string> SelectFields;
        /// <summary>
        /// 设置选择的字段
        /// </summary>
        public List<string> WhereConditions;
        private string parameterName => "shParamter" + parameters.Count;
        /// <summary>
        /// sql的部分类型
        /// </summary>
        private SqlOperationPartType sqlOperationPart;

        public DatabaseType DatabaseType { get;}
        public SqlInfo(DatabaseType databaseType)
        {
            SelectFields = new List<string>();
            WhereConditions=new List<string>(){"1==1"};

            this.DatabaseType = databaseType;
            parameters = new Dictionary<string, object>();
            allTableAlisa = new Queue<string>(all26Words);
        }

        /// <summary>
        /// 设置操作sql的哪一部分
        /// </summary>
        /// <param name="newSqlOperationPart"></param>
        public void SetSqlOperationPartType(SqlOperationPartType newSqlOperationPart)
        {
            this.sqlOperationPart = newSqlOperationPart;
        }

        public void AddSql(string sqlString)
        {
            switch (this.sqlOperationPart)
            {
                case SqlOperationPartType.Select:
                    SelectFields.Add(sqlString);
                    break;
                case SqlOperationPartType.Where:
                    whereSql +=" "+ sqlString;
                    break;
            }


            //sql += " " + sqlString;
        }

        public void AddParameter(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                sql += " null";
            }
            else
            {
                this.AddSql(parameterName);
                this.parameters.Add(parameterName, value);
            }

        }

        public string GetSql()
        {
            return whereSql;
        }
    }
}