using System.Collections.Generic;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class MultiQueryContext<T>
    {
        public List<JoinBodyBase<T>> JoinItems { set; get; } = new List<JoinBodyBase<T>>();

        public object MultiQuerySelectItem { set; get; }
        public object MultiQuerySelectAutoFillItem { set; get; }

        public List<MultiQueryOrderByItem> MultiQueryOrderByItems { set; get; } = new List<MultiQueryOrderByItem>();

        public object MultiQueryWhereItem { set; get; }
    }
}

