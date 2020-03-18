using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Core
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class PollyAttribute:Attribute
    {
        public int Retry { get; }
        public int RetryInterval { get; }
        public bool OpenCircuitBreaker { get; }
        public int ExceptionsAllowedBeforeBreaking { get; }

        public int DurationOfBreak { get; }
        public int Timeout { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="retry">重试次数</param>
        /// <param name="retryInterval">重试间隔，单位毫秒</param>
        /// <param name="openCircuitBreaker">是否开启断路</param>
        /// <param name="exceptionsAllowedBeforeBreaking">错误几次进入断路</param>
        /// <param name="durationOfBreak">断路时间</param>
        /// <param name="timeout">超时时间，单位毫秒</param>
        public PollyAttribute(int retry=0,int retryInterval=500,bool openCircuitBreaker=false,int exceptionsAllowedBeforeBreaking = 3, int durationOfBreak = 1000, int timeout=0)
        {
            Retry = retry;
            RetryInterval = retryInterval;
            OpenCircuitBreaker = openCircuitBreaker;
            ExceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            DurationOfBreak = durationOfBreak;
            Timeout = timeout;
        }
    }
}
