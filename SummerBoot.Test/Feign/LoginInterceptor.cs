using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SummerBoot.Feign;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Test.Feign
{

    public class LoginResultDto
    {
        public DateTime CreateTime { get; set; }
        public string Token { get; set; }
    }

    public class LoginDto
    {
        public string Password { get; set; }
        public string Name { get; set; }
    }

    [FeignClient(Url = "http://localhost:5001/login", IsIgnoreHttpsCertificateValidate = true,Timeout = 100)]
    public interface ILoginFeign
    {
        [PostMapping("/login")]
        Task<LoginResultDto> LoginAsync([Body()] LoginDto loginDto );
    }

    public class LoginInterceptor : IRequestInterceptor
    {

        private readonly ILoginFeign loginFeign;
        private readonly IConfiguration configuration;

        public LoginInterceptor(ILoginFeign loginFeign, IConfiguration configuration)
        {
            this.loginFeign = loginFeign;
            this.configuration = configuration;
        }
        

        public async Task ApplyAsync(RequestTemplate requestTemplate)
        {
            var username = configuration.GetSection("username").Value;
            var password = configuration.GetSection("password").Value;

            var loginResultDto = await this.loginFeign.LoginAsync(new LoginDto(){Name = username,Password = password});
            if (loginResultDto != null)
            {
                requestTemplate.Headers.Add("Authorization", new List<string>() { "Bearer "+loginResultDto.Token });
            }

            await Task.CompletedTask;
        }
    }
}
