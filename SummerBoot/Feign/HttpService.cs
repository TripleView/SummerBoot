using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using  Newtonsoft.Json;

namespace SummerBoot.Feign
{
    public class HttpService:FeignAspectSupport
    {
        public async Task<T> ExecuteAsync<T>(List<object> args, MethodInfo method,IServiceProvider serviceProvider)
        {
            return await base.BaseExecuteAsync<T>(method,args.ToArray(),serviceProvider);
        }
    }
}