using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{

    public interface ICustomCustomerRepository : IBaseRepository<Customer>
    {
        Task<List<Customer>> GetCustomersAsync(string name);
        Task<Customer> GetCustomerAsync(string name);
        Task<int> UpdateCustomerNameAsync(string oldName, string newName);

        Task<int> CustomQueryAsync();
    }

    [AutoRegister(typeof(ICustomCustomerRepository))]
    public class CustomCustomerRepository : BaseRepository<Customer>, ICustomCustomerRepository
    {
        public CustomCustomerRepository(IUnitOfWork uow, IDbFactory dbFactory, RepositoryOption repositoryOption) : base(uow, dbFactory, repositoryOption)
        {
        }

        public async Task<Customer> GetCustomerAsync(string name)
        {
            var result =
                await this.QueryFirstOrDefaultAsync<Customer>("select * from customer where name=@name", new { name });
            return result;
        }

        public async Task<List<Customer>> GetCustomersAsync(string name)
        {
            var result = await this.QueryListAsync<Customer>("select * from customer where name=@name", new { name });

            return result;
        }

        public async Task<int> UpdateCustomerNameAsync(string oldName, string newName)
        {
            var result = await this.ExecuteAsync("update customer set name=@newName where name=@oldName", new { newName, oldName });
            return result;
        }

        public async Task<int> CustomQueryAsync()
        {
            this.OpenDb();
            var grid = await this.dbConnection.QueryMultipleAsync("select id from customer",transaction:dbTransaction);
            var id = grid.Read().FirstOrDefault()?.id;
            this.CloseDb();
            return id;
        }
    }
}