namespace SummerBoot.Repository.Attributes
{
    public class DeleteAttribute : RepositoryActionAttribute
    {
        public DeleteAttribute(string sql, string path = "") : base(sql, path) { }
    }
}