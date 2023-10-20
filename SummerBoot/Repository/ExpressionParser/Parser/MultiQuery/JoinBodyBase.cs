using System.Linq.Expressions;
using System;

namespace SummerBoot.Repository.ExpressionParser.Parser.MultiQuery
{
    public class JoinBodyBase<T1>
    {
        /// <summary>
        /// join的类型
        /// </summary>
        public JoinType JoinType { get; set; }
        /// <summary>
        /// 源
        /// </summary>
        public IRepository<T1> Repository { get; set; }
        /// <summary>
        /// join的条件
        /// </summary>
        public object  Condition { get; set; }
        /// <summary>
        /// join表的类
        /// </summary>
        public Type JoinTable { get; set; }
       /// <summary>
       /// join表的别名
       /// </summary>
        public string JoinTableAlias { get; set; }
    }
    public class JoinBody2<T1, T2> : JoinBodyBase<T1>
    {
       
    }

    public class JoinBody3<T1, T2, T3> : JoinBodyBase<T1>
    {
      
    }

    public class JoinBody4<T1, T2, T3, T4> : JoinBodyBase<T1>
    {
    }
}