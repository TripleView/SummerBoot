using System;

namespace SummerBoot.Repository
{
    public class SelectAttribute: RepositoryActionAttribute
    {
        public SelectAttribute(string sql, string path = "") : base(sql,path){}

    }
}