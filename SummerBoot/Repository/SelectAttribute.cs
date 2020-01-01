using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Method,Inherited = true)]
    public class SelectAttribute:Attribute
    {
        public SelectAttribute(string sql)
        {
            this.Sql = sql;
        }
        public string Sql { private set; get; }
    }
}