using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using SummerBoot.Feign.Nacos.Dto;

namespace SummerBoot.Feign.Nacos
{
    /// <summary>
    /// nocas后台服务
    /// </summary>
    public class NacosBackgroundService: BackgroundService
    {
        private readonly INacosService nacosService;
        private readonly ILogger<NacosBackgroundService> logger;
        private readonly IConfiguration configuration;
        private string ip;

        public NacosBackgroundService(INacosService nacosService, ILogger<NacosBackgroundService> logger, IConfiguration configuration)
        {
            this.nacosService = nacosService;
            this.logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var namespaceId = configuration.GetSection("nacos:namespaceId").Value.GetValueOrDefault("public");
            var serviceName = configuration.GetSection("nacos:serviceName").Value;
            var groupName = configuration.GetSection("nacos:groupName").Value.GetValueOrDefault("DEFAULT_GROUP");
            var protocol = configuration.GetSection("nacos:protocol").Value.GetValueOrDefault("http");
            var portString = configuration.GetSection("nacos:port").Value.GetValueOrDefault("80");
            int.TryParse(portString, out var port);

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug(DateTime.Now.ToLongTimeString() + ": nacos send instance beat!");
                var beatBody = new SendInstanceHeartBeatInstanceInfoDto()
                {
                    Ip = ip,
                    Port = port,
                    ServiceName = serviceName,
                    NamespaceId = namespaceId,
                    Scheduled = false,
                    MetaData = new Dictionary<string, string>()
                    {
                        {"protocol",protocol}
                    }
                };
                try
                {
                    var result = await nacosService.SendInstanceHeartBeat(new SendInstanceHeartBeatDto()
                    {
                        ServiceName = serviceName,
                        Ephemeral = false,
                        Beat = beatBody
                    });
                    logger.LogDebug($"Send Instance HeartBeat,result{result}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                await Task.Delay(5000, stoppingToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            ip = SbUtil.GetCurrentIp();

            //var result = await nacosService.RegisterInstance(new NacosRegisterInstanceDto()
            //{
            //    Ip = ip,
            //    Port = 8888,
            //    ServiceName = "test",
            //    NamespaceId = "public"
            //});

            //if (result == "ok")
            //{
            //    logger.LogInformation("register instance to nacos server");
            //}

            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

       
    }
}