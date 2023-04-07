using SummerBoot.Repository;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    [SqliteAutoRepositoryAttribute]
    public interface IOrderHeaderRepository:IBaseRepository<OrderHeader>
    {

    }
}
