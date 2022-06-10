using Example.WebApi.Model;
using SummerBoot.Repository;

namespace Example.WebApi.Repository
{
    [AutoRepository]
    public interface ICustomerRepository:IBaseRepository<Customer>
    {
        
    }
}