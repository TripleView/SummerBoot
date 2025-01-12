using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SummerBoot.Cache;
using SummerBoot.Core;
using SummerBoot.Feign.Nacos.Dto;

namespace SummerBoot.Feign.Nacos;

//接着自定义登录拦截器
public class NacosLoginInterceptor : IRequestInterceptor
{

    private readonly INacosService nacosService;
    private readonly IConfiguration configuration;

    public NacosLoginInterceptor(INacosService nacosService, IConfiguration configuration)
    {
        this.nacosService = nacosService;
        this.configuration = configuration;
    }

    private void HandleUrl(RequestTemplate requestTemplate, NacosLoginOutputDto loginInfo)
    {
        if (!requestTemplate.Url.Contains("?"))
        {
            requestTemplate.Url += "?";
        }
        else
        {
            requestTemplate.Url += "&";
        }
        requestTemplate.Url += "accessToken=" + loginInfo.AccessToken;
    }

    public async Task ApplyAsync(RequestTemplate requestTemplate)
    {
        var cacheKey = "nacosLoginInfo";
        if (NacosUtil.FeignCache.TryGetValue(cacheKey, out var feignCacheEntity))
        {
            if (feignCacheEntity.IsEffective)
            {
                var loginResultDtoTemp = feignCacheEntity.Data as NacosLoginOutputDto;
                HandleUrl(requestTemplate, loginResultDtoTemp);
                return;
            }
        }

        var username = configuration.GetSection("nacos:username").Value;
        var password = configuration.GetSection("nacos:password").Value;
        if (username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
        {
            return;
        }

        var loginResultDto = await this.nacosService.LoginAsync(new NacosLoginInputDto() { UserName = username, Password = password });
        if (loginResultDto != null)
        {
            HandleUrl(requestTemplate, loginResultDto);
            NacosUtil.FeignCache.TryAdd(cacheKey, new FeignCacheEntity()
            {
                Data = loginResultDto,
                ExpirationTime = DateTime.Now.AddSeconds(loginResultDto.TokenTtl - 100)
            });
        }

        await Task.CompletedTask;

    }
}