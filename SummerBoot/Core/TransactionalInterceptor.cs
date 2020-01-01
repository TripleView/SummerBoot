using System;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace SqlOnline.Utils
{
    /// <summary>
    /// 事物拦截器
    /// </summary>
    public class TransactionalInterceptor : IInterceptor
    {
        [Autowired]
        private IUnitOfWork Uow { set; get; }
        [Autowired]
        private ILogger<TransactionalInterceptor> Logger { set; get; }

        public void Intercept(IInvocation invocation)
        {
            if (Uow == null)
            {
                invocation.Proceed();
                return;
            }

            var method = invocation.MethodInvocationTarget;
            if (method != null && method.GetCustomAttribute<TransactionalAttribute>() != null)
            {
                Uow.BeginTransaction();
            }
            else
            {
                invocation.Proceed();
                return;
            }

            var callMethodSuccess = true;
            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                callMethodSuccess = false;
                Uow.RollBack();
                Logger.LogDebug(e.Message);
            }

            if (callMethodSuccess && method.GetCustomAttribute<TransactionalAttribute>() != null)
            {
                Uow.Commit();
            }
        }
    }
}