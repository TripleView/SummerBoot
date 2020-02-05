using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class CacheInterceptor: CacheAspectSupport, IInterceptor
    {
        [Autowired]
        private IServiceProvider ServiceProvider { set; get; }

        public async void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget;

            invocation.ReturnValue= await base.Execute(() =>
            {
                invocation.Proceed();
                return invocation.ReturnValue;
            }, invocation.InvocationTarget, invocation.TargetType, method, invocation.Arguments, ServiceProvider);

        }
    }
}