using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RepositoryActionAttribute:Attribute
    {
        public RepositoryActionAttribute(string sql,string path="")
        {
            Sql = sql;
            Path = path;
        }
        public string Sql { set; get; }
        public string Path { set; get; }
    }
}
