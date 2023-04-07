using SummerBoot.Repository;
using WebApiExample.Model;

namespace WebApiExample.Repository
{
    [AutoRepository2]
    public interface ISqlServerCustomerRepository : IBaseRepository<Customer>
    {
     
    }
}