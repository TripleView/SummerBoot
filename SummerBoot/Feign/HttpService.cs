using Microsoft.Extensions.Logging;
using Polly;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign
{
    public class HttpService : FeignAspectSupport
    {

        public async Task<T> ExecuteAsync<T>(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));
            var feignClientAttribute = interfaceType.GetCustomAttribute<FeignClientAttribute>();
            if (feignClientAttribute == null)
            {
                throw new Exception(method.Name + " have not FeignClientAttribute");
            }

            var name = feignClientAttribute.Name;
            var fallBack = feignClientAttribute.FallBack;
            object interfaceTarget;
            //处理熔断重试超时逻辑
            IAsyncPolicy<T> policy = Policy.NoOpAsync<T>();
            var logFactory = serviceProvider.GetService<ILoggerFactory>();
            var log = logFactory.CreateLogger<HttpService>();

            var pollyAttribute = interfaceType.GetCustomAttribute<PollyAttribute>();

            if (pollyAttribute != null)
            {
                if (pollyAttribute.Retry > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(pollyAttribute.Retry, i => TimeSpan.FromMilliseconds(pollyAttribute.RetryInterval), (
                        (exception,timeSpan, retryCount, context) =>
                        {
                            log.LogError($"feign客户端{name}：开始第 " + retryCount + "次重试,当前时间:" + DateTime.Now);
                        })));
                }

                if (pollyAttribute.Timeout > 0)
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(() => TimeSpan.FromMilliseconds(pollyAttribute.Timeout), Polly.Timeout.TimeoutStrategy.Pessimistic, (
                        (context, span, arg3, arg4) =>
                        {
                            log.LogError($"feign客户端{name}：超时提醒,当前时间:" + DateTime.Now);
                            return Task.CompletedTask;
                        })));
                }

                if (pollyAttribute.OpenCircuitBreaker)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(pollyAttribute.ExceptionsAllowedBeforeBreaking, TimeSpan.FromMilliseconds(pollyAttribute.DurationOfBreak), (
                    //policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(1500), (
                        (exception, span, arg3) =>
                        {
                            log.LogError($"feign客户端{name}：熔断: {span.TotalMilliseconds } ms, 异常: " + exception.Message + DateTime.Now);
                        }), (context =>
                    {
                        log.LogError("feign客户端{name}：熔断器关闭了" + DateTime.Now);
                    }),
                        (() => { log.LogError("feign客户端{name}：熔断时间到，进入半开状态" + DateTime.Now); })));
                }

                return await policy.ExecuteAsync(async () => await base.BaseExecuteAsync<T>(method, args.ToArray(), serviceProvider));
            }

            return await base.BaseExecuteAsync<T>(method, args.ToArray(), serviceProvider);


        }
    }
}