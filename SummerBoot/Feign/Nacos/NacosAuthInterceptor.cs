using Microsoft.Extensions.Configuration;
using SummerBoot.Cache;
using SummerBoot.Feign.Nacos.Dto;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Feign.Nacos
{
    public class NacosAuthInterceptor
    {
        private readonly INacosService nacosService;
        private readonly IConfiguration configuration;
        private readonly ICache cache;

        public NacosAuthInterceptor(INacosService nacosService, IConfiguration configuration, ICache cache)
        {

            this.nacosService = nacosService;
            this.configuration = configuration;
            this.cache = cache;
        }


        public async Task ApplyAsync(RequestTemplate requestTemplate)
        {

            bool needAuth = false;

            var nacosSetting = configuration.GetSection("nacos");

            bool.TryParse(nacosSetting.GetSection("AuthEnabled").Value, out needAuth);

            if (needAuth)
            {
                string accessToken = string.Empty;

                var cachedToken = cache.GetValue<string>("nacosAccessToken");

                if (cachedToken.HasValue)
                {
                    accessToken = cachedToken.Data;
                }

                if (string.IsNullOrEmpty(accessToken))
                {

                    var username = nacosSetting.GetSection("UserName").Value;
                    var password = nacosSetting.GetSection("Password").Value;

                    var loginResultDto = await this.nacosService.QueryToken(new QueryTokenDto() { UserName = username, Password = password });
                    if (loginResultDto != null)
                    {
                        accessToken = loginResultDto.accessToken;
                    }
                    DateTime currentTime = DateTime.Now;
                    cache.SetValueWithAbsolute("nacosAccessToken", loginResultDto.accessToken, (currentTime.AddSeconds(loginResultDto.tokenTtl) - currentTime).Duration());

                }
                if (!string.IsNullOrEmpty(accessToken))
                {
                    requestTemplate.Url += ($"&accessToken={accessToken}");
                }
            }

            await Task.CompletedTask;
        }
    }

}
