namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class MultiQueryOrderByItem
    {
        /// <summary>
        /// 排序类型
        /// </summary>
        public OrderByType OrderByType { get; set; }
        /// <summary>
        /// 排序表达式
        /// </summary>
        public object OrderByExpression { get; set; }
        /// <summary>
        /// 是否是主排序
        /// </summary>
        public bool IsMain { get; set; }
    }
}

