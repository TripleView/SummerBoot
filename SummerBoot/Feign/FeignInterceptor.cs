using Castle.DynamicProxy;
using SummerBoot.Core;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public class FeignInterceptor : FeignAspectSupport, IInterceptor
    {
        [Autowired]
        private IServiceProvider ServiceProvider { set; get; }

        public async void  Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            var args = invocation.Arguments;

            if (method == null) return;
            var isAsyncType = method.ReturnType.IsAsyncType();
            if (isAsyncType)
            {
                invocation.ReturnValue = await base.ExecuteAsync(method, args, ServiceProvider);
            }
            else
            {
                invocation.ReturnValue = base.ExecuteAsync(method, args, ServiceProvider).GetAwaiter().GetResult();
            }
            
        }

	}
}