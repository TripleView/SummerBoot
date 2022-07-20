using SummerBoot.Repository;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    [AutoRepository]
    public interface IGuidModelRepository : IBaseRepository<GuidModel>
    {
        
    }
}