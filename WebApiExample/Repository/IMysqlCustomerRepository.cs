using SummerBoot.Repository;
using WebApiExample.Model;

namespace WebApiExample.Repository
{
    [AutoRepository1]
    public interface IMysqlCustomerRepository : IBaseRepository<Customer>
    {
    }
}