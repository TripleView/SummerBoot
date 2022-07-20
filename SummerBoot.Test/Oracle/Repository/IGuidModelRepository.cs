using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [AutoRepository]
    public interface IGuidModelRepository : IBaseRepository<GuidModel>
    {
        
    }
}