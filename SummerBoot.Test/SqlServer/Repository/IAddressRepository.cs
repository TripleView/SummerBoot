using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    [SqlServerAutoRepositoryAttribute]
    public interface IAddressRepository : IBaseRepository<Address>
    {
        
    }
}