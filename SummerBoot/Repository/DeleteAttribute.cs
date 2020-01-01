using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Method,Inherited = true)]
    public class DeleteAttribute : Attribute
    {
        public DeleteAttribute(string sql)
        {
            this.Sql = sql;
        }
        public string Sql { private set; get; }
    }
}