using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SummerBoot.Core.MvcExtension
{
    /// <summary>
    /// 参数校验过滤器
    /// </summary>
    public class ValidateParameterActionFilter : IActionFilter
    {
        /// <summary>
        /// action执行前
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //校验参数
            if (!context.ModelState.IsValid)
            {
                var errorMsg = context.ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                var result = ApiResult<string>.Ng(errorMsg);
                result.Msg = errorMsg;
                context.Result = new JsonResult(result);
            }
        }

        /// <summary>
        /// action执行后
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
