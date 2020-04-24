using System;
using System.Collections.Generic;
using System.Text;
using SummerBoot.Repository;
using SummerBoot.WebApi.Models;

namespace SummerBoot.WebApi.Repository
{
    [Repository]
    public interface IOrderDetailRepository:IRepository<OrderDetail>
    {
    }
}
