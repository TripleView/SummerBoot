using System;
using System.Collections.Generic;
using System.Text;
using SummerBoot.Repository;
using SummerBoot.Test.Models;

namespace SummerBoot.Test.Repository
{
    [Repository]
    public interface IOrderDetailRepository:IRepository<OrderDetail>
    {
    }
}
