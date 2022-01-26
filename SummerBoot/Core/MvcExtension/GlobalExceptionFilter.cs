using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SummerBoot.Core.MvcExtension
{
    
    /// <summary>
    /// 全局错误拦截器
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            this.logger.LogError(context.Exception,"");
            var msg = context.Exception.Message;
            
            var result = new ApiResult<string>() { Code = ApiResultCodeEnum.Ng, Msg =  msg };
            context.Result = new JsonResult(result);
            //context.ExceptionHandled = true;//异常已被处理
        }
    }
}
