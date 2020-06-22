using System;

namespace SummerBoot.Repository
{
    public class DeleteAttribute : RepositoryActionAttribute
    {
        public DeleteAttribute(string sql, string path = "") : base(sql, path) { }
    }
}