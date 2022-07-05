namespace SummerBoot.Core
{
    public enum ApiResultCodeEnum
    {
        Ok = 20000,
        Ng = 40000,
    }

    /// <summary>
    /// 通用返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 状态值枚举
        /// </summary>
        public ApiResultCodeEnum Code { set; get; }
        public string Msg { set; get; }
        public T Data { set; get; }

        public ApiResult()
        {

        }

        public ApiResult(ApiResultCodeEnum code, string msg, T data)
        {
            this.Code = code;
            this.Msg = msg;
            this.Data = data;
        }
        public static ApiResult<T> Ok(T data) => new ApiResult<T>(ApiResultCodeEnum.Ok, "", data);
        public static ApiResult<T> Ok(string msg, T data) => new ApiResult<T>(ApiResultCodeEnum.Ok, msg, data);

        public static ApiResult<T> Ng(T data) => new ApiResult<T>(ApiResultCodeEnum.Ng, "", data);
        public static ApiResult<T> Ng(string msg) => new ApiResult<T>(ApiResultCodeEnum.Ng, msg,default);
    }
}