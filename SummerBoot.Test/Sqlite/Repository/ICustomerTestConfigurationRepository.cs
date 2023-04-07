using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Repository
{
    [SqliteAutoRepositoryAttribute]
    public interface ICustomerTestConfigurationRepository : IBaseRepository<Customer>
    {
        //异步
        [Select("${oracleSql:QueryListSql}")]
        Task<List<Customer>> QueryListAsync();

        [Select("${oracleSql:QueryByPageSql}")]
        Task<Page<Customer>> QueryByPageAsync(IPageable pageable);
        //异步
        [Update("${oracleSql:UpdateByNameSql}")]
        Task<int> UpdateByNameAsync(string name, int age);

        [Delete("${oracleSql:DeleteByNameSql}")]
        Task<int> DeleteByNameAsync(string name);

        //同步
        [Select("${oracleSql:QueryListSql}")]
        List<Customer> QueryList();

        [Select("${oracleSql:QueryByPageSql}")]
        Page<Customer> QueryByPage(IPageable pageable);
        //异步
        [Update("${oracleSql:UpdateByNameSql}")]
        int UpdateByName(string name,int age);

        [Delete("${oracleSql:DeleteByNameSql}")]
        int DeleteByName(string name);
    }
}