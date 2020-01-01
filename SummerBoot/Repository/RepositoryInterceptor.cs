using Castle.DynamicProxy;
using Dapper;
using SummerBoot.Core;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SqlOnline.Utils;

namespace SummerBoot.Repository
{
    public class RepositoryInterceptor<T> : RepositoryAspectSupport<T>, IInterceptor
    {
        [Autowired]
        private IServiceProvider ServiceProvider { set; get; }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method == null) return;

            invocation.ReturnValue=base.Execute(() =>
            {
                invocation.Proceed();
                return invocation.ReturnValue;
            }, method, invocation.Arguments, ServiceProvider);

        }
    }
}