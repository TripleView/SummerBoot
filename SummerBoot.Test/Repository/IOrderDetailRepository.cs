using System;
using System.Collections.Generic;
using System.Text;
using SummerBoot.Repository;
using SummerBoot.WebApi.Models;

namespace SummerBoot.Test.Repository
{
    [AutoRepository]
    public interface IOrderDetailRepository:IBaseRepository<OrderDetail>
    {
    }
}
