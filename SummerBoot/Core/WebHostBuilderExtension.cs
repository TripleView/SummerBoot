using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace SummerBoot.Core
{
    /// <summary>
    /// web host扩展
    /// </summary>
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder UseSummerBoot(this IWebHostBuilder webHostBuilder)
        {
            
            var env = webHostBuilder.GetSetting("ENVIRONMENT");
            
            var port = 5000;
            var configJsonFile = "appsettings.json";
            if (env.ToLower() != "production")
            {
                configJsonFile = $"appsettings.{env}.json";
            }
           
            
            var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(configJsonFile)
                .Build();

            //server
            var ip = configuration.GetSection("server:ip")?.Value??"*";

            //端口号
            var portString = configuration.GetSection("server:port")?.Value;
            if (portString.HasText() && int.TryParse(portString, out var tempPort))
            {
                port = tempPort;
            }

            var contentRoot = AppContext.BaseDirectory;
            webHostBuilder.UseUrls($"http://{ip}:{port}").UseContentRoot(contentRoot);
            return webHostBuilder;
        }
    }
}
