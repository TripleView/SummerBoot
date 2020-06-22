using System;

namespace SummerBoot.Repository
{
    public class InsertAttribute : RepositoryActionAttribute
    {
        public InsertAttribute(string sql, string path = "") : base(sql, path) { }
    }
}