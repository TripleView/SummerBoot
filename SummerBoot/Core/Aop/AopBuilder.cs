using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using SummerBoot.Core.Aop.Express;
using SummerBoot.Core.Aop.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SqlOnline.Utils;
using StackExchange.Redis;
using SummerBoot.Repository;

namespace SummerBoot.Core.Aop
{
    public class AopBuilder
    {
        public IServiceCollection Service { get; }

        private readonly IList<Type> _types;

        public AopBuilder(IServiceCollection service)
        {
            Service = service;
            _types = SbUtil.GetAllTypes().Where(t => t.IsClass && !t.IsAbstract).ToList();
        }

        public IEnumerable<Type> FilterType(Func<Type,bool> func)
        {
            return _types.Where(func);
        }
    }
}