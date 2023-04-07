using SummerBoot.Repository;
using WebApiExample.Model;

namespace WebApiExample.Repository
{
    [AutoRepository1]
    public interface ICustomerRepository : IBaseRepository<Customer>
    {

    }
}
