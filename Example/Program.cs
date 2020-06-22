using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demo.Service;
using Example.Feign;
using Example.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using SummerBoot.Feign;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(it =>
                {
                    it.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>().UseUrls("http://*:5000"); });
    }
}
