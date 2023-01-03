using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using WebApiExample.Model;
using WebApiExample.Repository;

namespace WebApiExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MultipleDatabaseController : ControllerBase
    {
        private readonly IUnitOfWork1 mysqlUow;
        private readonly IUnitOfWork1 sqlServerUow;
        private readonly IDbGenerator1 mysqlDbGenerator;
        private readonly IDbGenerator2 sqlServerDbGenerator;
        private readonly ISqlServerCustomerRepository sqlServerCustomerRepository;
        private readonly IMysqlCustomerRepository mysqlCustomerRepository;

        public MultipleDatabaseController(IUnitOfWork1 mysqlUow, IUnitOfWork1 sqlServerUow, IDbGenerator1 mysqlDbGenerator, IDbGenerator2 sqlServerDbGenerator, ISqlServerCustomerRepository sqlServerCustomerRepository, IMysqlCustomerRepository mysqlCustomerRepository)
        {
            this.mysqlUow = mysqlUow;
            this.sqlServerUow = sqlServerUow;
            this.mysqlDbGenerator = mysqlDbGenerator;
            this.sqlServerDbGenerator = sqlServerDbGenerator;
            this.sqlServerCustomerRepository = sqlServerCustomerRepository;
            this.mysqlCustomerRepository = mysqlCustomerRepository;
        }

        /// <summary>
        /// 生成数据库
        /// </summary>
        /// <returns></returns>
        [HttpGet("GenerateTable")]
        public string GenerateTable()
        {
            var sqlResults = mysqlDbGenerator.GenerateSql(new List<Type>() { typeof(Customer) });
            foreach (var result in sqlResults)
            {
                mysqlDbGenerator.ExecuteGenerateSql(result);
            }

            var sqlResults2 = sqlServerDbGenerator.GenerateSql(new List<Type>() { typeof(Customer) });
            foreach (var result in sqlResults2)
            {
                sqlServerDbGenerator.ExecuteGenerateSql(result);
            }
            return "ok";
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpGet("MultiQuery")]
        public async Task<string> MultiQuery()
        {

            var customer = new Customer() { Name = "testCustomer" };
            await sqlServerCustomerRepository.InsertAsync(customer);
            await mysqlCustomerRepository.InsertAsync(customer);

            try
            {
                sqlServerUow.BeginTransaction();
                var customer2 = new Customer() { Name = "testCustomer2" };
                await sqlServerCustomerRepository.InsertAsync(customer2);
                var customer3 = new Customer() { Name = "testCustomer3" };
                await sqlServerCustomerRepository.InsertAsync(customer3);
                sqlServerUow.Commit();
            }
            catch (Exception e)
            {
                sqlServerUow.RollBack();
            }

            try
            {
                mysqlUow.BeginTransaction();
                var customer2 = new Customer() { Name = "testCustomer2" };
                await mysqlCustomerRepository.InsertAsync(customer2);
                var customer3 = new Customer() { Name = "testCustomer3" };
                await mysqlCustomerRepository.InsertAsync(customer3);
                throw new Exception("test");
                mysqlUow.Commit();
            }
            catch (Exception e)
            {
                mysqlUow.RollBack();
            }

            var sqlServerCount = sqlServerCustomerRepository.Count();
            var mysqlCount = mysqlCustomerRepository.Count();

            return "ok";
        }


    }
}