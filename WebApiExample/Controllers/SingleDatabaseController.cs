using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using WebApiExample.Model;
using WebApiExample.Repository;

namespace WebApiExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SingleDatabaseController : ControllerBase
    {
        private readonly IUnitOfWork1 uow;
        private readonly ICustomerRepository customerRepository;
        private readonly IDbGenerator1 dbGenerator1;
        private readonly IOrderHistoryRepository orderHistoryRepository;
        private readonly IConfiguration configuration;

        public SingleDatabaseController(IUnitOfWork1 uow, ICustomerRepository customerRepository, IDbGenerator1 dbGenerator1, IOrderHistoryRepository orderHistoryRepository, IConfiguration configuration)
        {
            this.uow = uow;
            this.customerRepository = customerRepository;
            this.dbGenerator1 = dbGenerator1;
            this.orderHistoryRepository = orderHistoryRepository;
            this.configuration = configuration;
        }


        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var c = configuration["a"];
            return Content(c);
        }

        /// <summary>
            /// 生成数据库
            /// </summary>
            /// <returns></returns>
            [HttpGet("GenerateTable")]
        public string GenerateTable()
        {
            var sqlResults = dbGenerator1.GenerateSql(new List<Type>() { typeof(Customer), typeof(OrderHistory) });
            foreach (var result in sqlResults)
            {
                dbGenerator1.ExecuteGenerateSql(result);
            }

            return "ok";
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpGet("Query")]
        public async Task<string> Query()
        {

            //常规查询
            var customers = customerRepository.Where(it => it.Age > 5).OrderBy(it => it.Id).Take(10).ToList();
            //分页
            var page2 = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();
            return "ok";
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <returns></returns>
        [HttpGet("Insert")]
        public async Task<string> Insert()
        {
            var customer = new Customer() { Name = "testCustomer" };
            var result = await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            var result2 = await customerRepository.InsertAsync(customerList);

            return "ok";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpGet("Delete")]
        public async Task<string> Delete()
        {
            var customer = new Customer() { Name = "testDeleteCustomer" };
            var result = await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            var result2 = await customerRepository.InsertAsync(customerList);
            var customer4 = new Customer() { Name = "customer4", Age = 20 };
            await customerRepository.InsertAsync(customer4);


            await customerRepository.DeleteAsync(result);
            await customerRepository.DeleteAsync(result2);
            var deleteCount = await customerRepository.DeleteAsync(it => it.Age > 5);

            return "ok";
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpGet("Update")]
        public async Task<string> Update()
        {
            var customer = new Customer() { Name = "testUpdateCustomer" };
            var result = await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            var result2 = await customerRepository.InsertAsync(customerList);
            var customer4 = new Customer() { Name = "customer4", Age = 20 };
            await customerRepository.InsertAsync(customer4);

            customer.Age = 100;
            customerRepository.Update(customer);

            foreach (var customer1 in customerList)
            {
                customer1.Age = 200;
            }
            customerRepository.Update(customerList);

            var updateCount = await customerRepository.Where(it => it.Name == "customer4")
                .SetValue(it => it.Age, 5)
                .SetValue(it => it.TotalConsumptionAmount, 100)
                .ExecuteUpdateAsync();

            return "ok";
        }

        /// <summary>
        /// 事务
        /// </summary>
        /// <returns></returns>
        [HttpGet("Transaction")]
        public async Task<string> Transaction()
        {
            try
            {
                uow.BeginTransaction();
                var customer = new Customer() { Name = "testCustomer2" };
                await customerRepository.InsertAsync(customer);
                var orderHistory = new OrderHistory()
                {
                    ProductName = "orage",
                    CustomerId = customer.Id
                };
                await orderHistoryRepository.InsertAsync(orderHistory);
                uow.Commit();
            }
            catch (Exception e)
            {
                uow.RollBack();
            }

            return "ok";
        }
    }
}