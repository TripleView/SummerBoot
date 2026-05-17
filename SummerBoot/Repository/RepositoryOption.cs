using SummerBoot.Core;
using SummerBoot.Repository.TypeHandler;
using SummerBoot.Repository.TypeHandler.Dialect.Oracle;
using SummerBoot.Repository.TypeHandler.Dialect.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using SummerBoot.Repository.TypeHandler.Dialect.SqlServer;

namespace SummerBoot.Repository
{
    public class RepositoryOption
    {
        /// <summary>
        /// sql语句里的参数标识符
        /// </summary>
        public static List<char> ParameterIdentifiers = new List<char>() { '@', ':', '?' };


        /// <summary>
        /// 数据库与实体类自动转换配置，对单表单字段进行特殊配置
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> generatorMap = new Dictionary<string, Dictionary<string, string>>();


        /// <summary>
        /// 数据库与实体类自动转换配置，创建自定义字段映射，对单表单字段进行配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <param name="databaseMappingType"></param>
        private void GeneratorMap<T>(Expression<Func<T, object>> selector, string databaseMappingType)
        {
            if (selector.Body is MemberExpression memberExpression)
            {
                var memberInfo = memberExpression.Member;
                var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();

                var columnName = columnAttribute?.Name ?? memberInfo.Name;
                var tableType = memberExpression.Member.DeclaringType;
                if (tableType == null)
                {
                    return;
                }
                var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
                var tableName = tableAttribute?.Name ?? tableType.Name;
                if (generatorMap.ContainsKey(tableName))
                {
                    var tableFieldMaps = generatorMap[tableName];
                    tableFieldMaps[columnName] = databaseMappingType;
                }
                else
                {
                    generatorMap[tableName] = new Dictionary<string, string>()
                    {
                        {columnName, databaseMappingType}
                    };
                }
            }
            else
            {
                throw new NotSupportedException("not support this selector");
            }
        }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>

        /// <summary>
        /// 全局唯一实例
        /// </summary>
        public static RepositoryOption Instance { set; get; }

        /// <summary>
        /// 是否使用软删除
        /// </summary>
        public bool IsUseSoftDelete { get; set; }

        public Dictionary<string, DatabaseUnit> DatabaseUnits { get; } = new Dictionary<string, DatabaseUnit>();

        public void AddDatabaseUnit<TDbConnection, TUnitOfWork>(string connectionString, Action<DatabaseUnit> optionAction) where TDbConnection : IDbConnection where TUnitOfWork : IUnitOfWork
        {
            if (connectionString.IsNullOrWhiteSpace())
            {
                throw new Exception("ConnectionString can not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(TUnitOfWork), typeof(TDbConnection), connectionString);
            
            //oracle
            if (databaseUnit.IsOracle || databaseUnit.IsMysql)
            {
          
                databaseUnit.SetTypeHandler(typeof(TimeSpan), new TimeSpanTypeHandler());
                if (databaseUnit.IsOracle)
                {
                    databaseUnit.SetParameterTypeMap(typeof(Guid), DbType.Binary);
                    databaseUnit.SetParameterTypeMap(typeof(bool), DbType.Byte);
                    databaseUnit.SetTypeHandler(typeof(bool), new BoolNumericTypeHandler());
                    databaseUnit.SetTypeHandler(typeof(Guid), new OracleGuidTypeHandler());
                }
                else
                {
                    databaseUnit.SetTypeHandler(typeof(Guid), new GuidTypeHandler());
                }
            }

            if (databaseUnit.IsSqlite)
            {
       
                databaseUnit.SetTypeHandler(typeof(Guid), new SqliteGuidTypeHandler());
                databaseUnit.SetTypeHandler(typeof(TimeSpan), new SqliteTimeSpanTypeHandler());
            }
            if (databaseUnit.IsSqlServer)
            {
                databaseUnit.SetParameterTypeMap(typeof(DateTime), DbType.DateTime2);
                databaseUnit.SetTypeHandler(typeof(TimeSpan), new TimeSpanTypeHandler());
            }

            if (databaseUnit.IsPgsql)
            {
                databaseUnit.SetTypeHandler(typeof(TimeSpan), new TimeSpanTypeHandler());
            }

            optionAction(databaseUnit);
            
            if (DatabaseUnits.ContainsKey(connectionString))
            {
                throw new RepeatAddDatabaseUnitException();
            }

            databaseUnit.BindDatabaseSpecificProviderType<DefaultDatabaseSpecificProvider>();
            DatabaseUnits[databaseUnit.ConnectionString] = databaseUnit;
            
        }
    }


    
}