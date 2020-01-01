using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Method,Inherited = true)]
    public class InsertAttribute : Attribute
    {
        public InsertAttribute(string sql)
        {
            this.Sql = sql;
        }
        public string Sql { private set; get; }
    }
}