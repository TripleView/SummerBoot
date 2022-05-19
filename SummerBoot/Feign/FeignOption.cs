namespace SummerBoot.Feign
{
    /// <summary>
    /// feign配置类
    /// </summary>
    public class FeignOption
    {
        /// <summary>
        /// 是否开启nacos微服务模式
        /// </summary>
        public bool EnableNacos { get; set; }
        /// <summary>
        /// 是否将本机注册为服务实例
        /// </summary>
        public bool NacosRegisterInstance { get; set; }
    }
}