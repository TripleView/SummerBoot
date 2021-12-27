using System;

namespace SummerBoot.Repository.Attributes
{
    public class UpdateAttribute : RepositoryActionAttribute
    {
        public UpdateAttribute(string sql, string path = "") : base(sql, path) { }
    }
}