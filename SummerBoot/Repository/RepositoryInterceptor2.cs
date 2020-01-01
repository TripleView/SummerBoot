using Castle.DynamicProxy;
using SummerBoot.Core;
using System;

namespace SummerBoot.Repository
{
    public class RepositoryInterceptor2 : RepositoryAspectSupport2, IInterceptor
    {
        [Autowired]
        private IServiceProvider ServiceProvider { set; get; }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method == null) return;

            var args = invocation.Arguments;

            invocation.ReturnValue = base.Execute(method, args, ServiceProvider);

        }
    }
}