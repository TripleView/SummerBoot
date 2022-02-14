using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Mysql.Models;

namespace SummerBoot.Test.Mysql.Repository
{
    [AutoRepository]
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        //异步
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
        Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);
        
        [Select("select * from customer where age>@age order by id")]
        Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);
        
        //同步
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
        List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

        [Select("select * from customer where age>@age order by id")]
        Page<Customer> GetCustomerByPage(IPageable pageable, int age);

        /// <summary>
        /// where构造条件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}}")]
        Task<List<CustomerBuyProduct>> GetCustomerByConditionAsync(WhereItem name, WhereItem age);

        [Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}} order by id")]
        Task<Page<Customer>> GetCustomerByPageByConditionAsync(IPageable pageable, WhereItem name, WhereItem age);

        [Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}}")]
        List<CustomerBuyProduct> GetCustomerByCondition(WhereItem name, WhereItem age);

        [Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}} order by id")]
        Page<Customer> GetCustomerByPageByCondition(IPageable pageable, WhereItem  name, WhereItem age);
    }
}