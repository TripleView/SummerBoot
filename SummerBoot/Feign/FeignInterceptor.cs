using Castle.DynamicProxy;
using SummerBoot.Core;
using System;
using System.Linq;
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
            var returnType =method.ReturnType;
            var isAsyncType = method.ReturnType.IsAsyncType();
            //if (isAsyncType)
            //{
                
            //    var proceedIno= invocation.CaptureProceedInfo();
                
            //    var result= await base.ExecuteAsync(method, args, ServiceProvider);
           
                


            //    invocation.ReturnValue = result;
                
            //}
            //else
            //{
            //    invocation.ReturnValue = base.ExecuteAsync(method, args, ServiceProvider).GetAwaiter().GetResult();
            //}
            
        }

	}
}