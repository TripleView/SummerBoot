using System;

namespace SummerBoot.Repository.Attributes
{
    public class SelectAttribute: RepositoryActionAttribute
    {
        public SelectAttribute(string sql, string path = "") : base(sql,path){}

    }
}