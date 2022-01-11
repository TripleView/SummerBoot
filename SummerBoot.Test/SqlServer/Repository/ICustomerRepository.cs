using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.SqlServer.Repository
{
    [AutoRepository]
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        //异步
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=:name")]
        Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);

        [Select("select * from customer where age>:age order by id")]
        Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);

        //同步
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=:name")]
        List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

        [Select("select * from customer where age>:age order by id")]
        Page<Customer> GetCustomerByPage(IPageable pageable, int age);

    }
}