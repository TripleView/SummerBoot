﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Test.SqlServer.Models;

namespace SummerBoot.Test.SqlServer.Repository
{
    [SqlServerAutoRepositoryAttribute]
    public interface ICustomerTestConfigurationRepository : IBaseRepository<Customer>
    {
        //异步
        [Select("${mysqlSql:QueryListSql}")]
        Task<List<Customer>> QueryListAsync();

        [Select("${mysqlSql:QueryByPageSql}")]
        Task<Page<Customer>> QueryByPageAsync(IPageable pageable);
        //异步
        [Update("${mysqlSql:UpdateByNameSql}")]
        Task<int> UpdateByNameAsync(string name, int age);

        [Delete("${mysqlSql:DeleteByNameSql}")]
        Task<int> DeleteByNameAsync(string name);

        //同步
        [Select("${mysqlSql:QueryListSql}")]
        List<Customer> QueryList();

        [Select("${mysqlSql:QueryByPageSql}")]
        Page<Customer> QueryByPage(IPageable pageable);
        //异步
        [Update("${mysqlSql:UpdateByNameSql}")]
        int UpdateByName(string name,int age);

        [Delete("${mysqlSql:DeleteByNameSql}")]
        int DeleteByName(string name);
    }
}