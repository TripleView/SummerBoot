using Microsoft.Extensions.Configuration;

namespace SummerBoot.Feign.Nacos
{
    public class NacosConfigurationSource : IConfigurationSource
    {
        private IConfiguration configuration;

        public NacosConfigurationSource(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new NacosConfigurationProvider(configuration);
        }
    }
}