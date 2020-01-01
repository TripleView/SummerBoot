using System;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Method,Inherited = true)]
    public class UpdateAttribute : Attribute
    {
        public UpdateAttribute(string sql)
        {
            this.Sql = sql;
        }
        public string Sql { private set; get; }
    }
}