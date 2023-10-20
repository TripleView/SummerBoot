using SummerBoot.Repository;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [MysqlAutoRepositoryAttribute]
    public interface IAddressRepository : IBaseRepository<Address>
    {
        
    }
}