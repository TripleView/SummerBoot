using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Oracle.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface ICustomerRepository :IBaseRepository<Customer>
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

        /// <summary>
        /// where构造条件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Select("select * from customer where 1=1 {{ and name=:name}}{{ and age=:age}}")]
        Task<List<CustomerBuyProduct>> GetCustomerByConditionAsync(WhereItem<string> name, WhereItem<int> age);

        [Select("select * from customer where 1=1 {{ and name=:name}}{{ and age=:age}} order by id")]
        Task<Page<Customer>> GetCustomerByPageByConditionAsync(IPageable pageable, WhereItem<string> name, WhereItem<int> age);

        [Select("select * from customer where 1=1 {{ and name=:name}}{{ and age=:age}}")]
        List<CustomerBuyProduct> GetCustomerByCondition(WhereItem<string> name, WhereItem<int> age);

        [Select("select * from customer where 1=1 {{ and name=:name}}{{ and age=:age}} order by id")]
        Page<Customer> GetCustomerByPageByCondition(IPageable pageable, WhereItem<string> name, WhereItem<int> age);
    }

}