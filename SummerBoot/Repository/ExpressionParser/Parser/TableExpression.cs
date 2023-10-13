using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// 代表一张表的表达式
    /// </summary>
    public class TableExpression : DbBaseExpression
    {
        /// <summary>
        /// 代表一张表的表达式
        /// </summary>
        /// <param name="type">表内元素的类型(对应实体类)</param>
        /// <param name="name">表的名称</param>
        public TableExpression(Type type)
            : base((ExpressionType)DbExpressionType.Table, type)
        {
        }

        public TableExpression(Type type,string tableAlias)
            : base((ExpressionType)DbExpressionType.Table, type)
        {
            this.TableAlias = tableAlias;
        }

        /// <summary>
        /// 表格别名
        /// </summary>
        public string TableAlias { get; set; }

        /// <summary>
        /// 表的名称
        /// </summary>
        public string Name 
        {
            get
            {
                //查找tableAttribute特性,看下有没有自定义表名
                var tableAttribute =Type.GetCustomAttribute<TableAttribute>();
                //如果没有该特性，直接使用类名作为表名
                var tableName = tableAttribute == null ? Type.Name : tableAttribute.Name;
                return tableName;
            }
        }

        /// <summary>
        /// 表的命名空间
        /// </summary>
        public string Schema
        {
            get
            {
                //查找tableAttribute特性,看下有没有自定义表名
                var tableAttribute = Type.GetCustomAttribute<TableAttribute>();
                //如果没有该特性，直接使用类名作为表名
                var schema = tableAttribute == null ? "" : tableAttribute.Schema;
                return schema;
            }
        }
        /// <summary>
        /// 表的列
        /// </summary>
        public List<ColumnExpression> Columns
        {
            get
            {
                if (_columns == null)
                {
                    int i = 0;
                    _columns = new List<ColumnExpression>();
                 
                    //排除掉不映射的列
                    var properties = Type.GetProperties().Where(it => !it.GetCustomAttributes().OfType<NotMappedAttribute>().Any()).ToList();
                    var tableAlias=TableAlias.HasText()?TableAlias:"";
                    foreach (var propertyInfo in properties)
                    {
                        _columns.Add(new ColumnExpression(propertyInfo.PropertyType,
                            tableAlias, propertyInfo, i++)
                        {
                            ValueType = propertyInfo.PropertyType
                        });
                    }
                    
                }

                return _columns;
            }
        }

        private List<ColumnExpression> _columns;
    }
}