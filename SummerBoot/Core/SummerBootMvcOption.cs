namespace SummerBoot.Core
{
    public class SummerBootMvcOption
    {
        /// <summary>
        /// 是否启用全局错误处理
        /// </summary>
        public bool UseGlobalExceptionHandle { get; set; } = false;
        /// <summary>
        /// 是否启用全局参数校验处理，返回统一格式
        /// </summary>
        public bool UseValidateParameterHandle { get; set; }
    }
}