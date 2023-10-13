using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    /// <summary>
    /// Select 表达式
    /// </summary>
    public class SelectExpression : QueryExpression
    {
        public SelectExpression(Type type, string alias, List<ColumnExpression> columns, Expression from, WhereExpression where = null,
            List<GroupByExpression> groupBy = null, List<OrderByExpression> orderBy = null, List<JoinExpression> joins = null)
            : base((ExpressionType)DbExpressionType.Select, type)
        {
            Alias = alias;
            Columns = columns;
            From = from;
            Where = where;
            GroupBy = groupBy ?? new List<GroupByExpression>();
            OrderBy = orderBy ?? new List<OrderByExpression>();
            Joins = joins ?? new List<JoinExpression>();
        }

        /// <summary>
        /// 是否忽略order by语句
        /// </summary>
        public bool IsIgnoreOrderBy { get; set; } = false;

        public SelectExpression(Type type, string alias, List<ColumnExpression> columns, Expression from,string columnsPrefix, int? skip = null, int? take = null, WhereExpression where = null,
            List<GroupByExpression> groupBy = null, List<OrderByExpression> orderBy = null)
            : this(type, alias, columns, from, where, groupBy, orderBy)
        {
            this.ColumnsPrefix = columnsPrefix;
            this.Skip = skip;
            this.Take = take;
        }
        #region 属性
        /// <summary>
        /// 所有列的前缀，比如DISTINCT
        /// </summary>
        public string ColumnsPrefix { get; set; }

        /// <summary>
        /// 跳过多少数据
        /// </summary>
        public int? Skip { get; set; }
        /// <summary>
        /// 取多少数据
        /// </summary>
        public int? Take { get; set; }
        /// <summary>
        /// 判断是否存在分页
        /// </summary>
        public bool HasPagination => Skip.HasValue || Take.HasValue;

        public bool HasGroupBy => GroupBy.Count > 0;
        /// <summary>
        /// Where条件
        /// </summary>
        public WhereExpression Where { get; set; }
        /// <summary>
        /// join other table
        /// </summary>
        public List<JoinExpression> Joins { get; set; }
        /// <summary>
        /// GroupBy
        /// </summary>
        public List<GroupByExpression> GroupBy { get; set; }

        /// <summary>
        /// OrderBy
        /// </summary>
        public List<OrderByExpression> OrderBy { get; set; }

        #endregion

        public SelectExpression DeepClone()
        {
            var result = new SelectExpression(this.Type, this.Alias, this.Columns, this.From,this.ColumnsPrefix, this.Skip, this.Take, this.Where, this.GroupBy,
                this.OrderBy);
            return result;
        }

    }
}