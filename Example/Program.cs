using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var Username = "token-zpz5g";
            var Password = "l5v5r8v2j5f47cvd9lqxvhn5t4grr2np676jxxcj4bj6p4x8jx2vf4";
            //var d = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
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
