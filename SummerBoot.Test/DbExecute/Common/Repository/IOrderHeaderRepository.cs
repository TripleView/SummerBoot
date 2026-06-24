using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.DbExecute.Common.Repository
{
    [AutoRepository]
    public interface IOrderHeaderRepository : IBaseRepository<OrderHeader>
    {
        [Select("${QueryFirstSql}")]
        Task<OrderHeader> SelectQueryAsync(string orderNo);
    }
}
