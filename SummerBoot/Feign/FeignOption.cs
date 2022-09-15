using Microsoft.Extensions.Configuration;

namespace SummerBoot.Feign
{
    /// <summary>
    /// feign配置类
    /// </summary>
    public class FeignOption
    {
        public IConfiguration Configuration { get; private set; }
        /// <summary>
        /// 是否开启nacos微服务模式
        /// </summary>
        public bool EnableNacos { get; private set; }
        /// <summary>
        /// 是否开启nacos配置读取
        /// </summary>
        public bool EnableNacosConfiguration { get; private set; }
        /// <summary>
        /// 添加nacos支持
        /// </summary>
        /// <param name="configuration"></param>
        public void AddNacos(IConfiguration configuration,bool enableNacosConfiguration=false)
        {
            Configuration = configuration;
            this.EnableNacos = true;
            this.EnableNacosConfiguration = enableNacosConfiguration;
        }
    }
}