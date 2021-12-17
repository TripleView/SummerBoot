using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.WebApi.Models;

namespace SummerBoot.Test.Repository
{
    [AutoRepository]
    public interface ICustomerRepository :IBaseRepository<Customer>
    {
        //一部
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
        Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);

        [Select("select * from customer where age>@age order by id")]
        Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);

        [Update("update customer set name=@name where age>@age")]
        Task<int> UpdateCustomerAsync(string name,int age);
        
        [Update("update customer set name=@name where age>@age")]
        Task UpdateCustomerTask(string name, int age);

        [Delete("delete from customer where age>@age")]
        Task<int> DeleteCustomerAsync( int age);

        [Delete("delete from customer where age>@age")]
        Task DeleteCustomerTask(int age);

        [Select("select CreateTime from orderHeader where OrderNo='fffdsfdsf'")]
        Task<DateTime> GetDatetime();
        
        //同步
        [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
                " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
        List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

        [Select("select * from customer where age>@age order by id")]
        Page<Customer> GetCustomerByPage(IPageable pageable, int age);

        [Update("update customer set name=@name where age>@age")]
        int UpdateCustomer(string name, int age);

        [Update("update customer set name=@name where age>@age")]
        void UpdateCustomerVoid(string name, int age);

        [Delete("delete from customer where age>@age")]
        int DeleteCustomer(int age);

        [Delete("delete from customer where age>@age")]
        void DeleteCustomerNoReturn(int age);
    }
}