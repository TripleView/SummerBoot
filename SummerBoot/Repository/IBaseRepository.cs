namespace SummerBoot.Repository
{
    public interface IBaseRepository<T> : ExpressionParser.Parser.IRepository<T> where T : class
    {
      
    }
}