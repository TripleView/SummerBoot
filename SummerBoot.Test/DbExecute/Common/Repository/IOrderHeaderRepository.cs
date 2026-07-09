using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.DbExecute.Common.Repository
{
    [AutoRepository]
    public interface IOrderHeaderRepository : IBaseRepository<OrderHeader>
    {
        [Select("${TestSelectAttributeSql}")]
        Task<OrderHeader> TestSelectAttributeAsync(string orderNo,int customerId);

        [Select("${TestSelectAttributePageSql}")]
        Task<Page<OrderHeader>> TestSelectAttributePageAsync(string orderNo,Pageable pageable);

        [Update("${TestUpdateAttributeSql}")]
        Task<int> TestUpdateAttributeAsync(string orderNo, int customerId);

        [Delete("${TestDeleteAttributeSql}")]
        Task<int> TestDeleteAttributeAsync(string orderNo, int customerId);
        
    }
}
