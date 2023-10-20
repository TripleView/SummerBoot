﻿using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface IAddressRepository : IBaseRepository<Address>
    {
        
    }
}