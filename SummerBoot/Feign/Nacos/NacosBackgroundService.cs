using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using SummerBoot.Feign.Nacos.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SummerBoot.Feign.Nacos
{
    /// <summary>
    /// nocas后台服务
    /// </summary>
    public class NacosBackgroundService: BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<NacosBackgroundService> logger;
        private readonly IConfiguration configuration;
        private readonly NacosOption nacosOption;
        private string ip;
        private Timer timer;
        public NacosBackgroundService(IServiceProvider serviceProvider, ILogger<NacosBackgroundService> logger, IConfiguration configuration, IOptions<NacosOption> nacosOptions)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
            this.nacosOption = nacosOptions.Value.GetConfigurationValueOrDefault();
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        /// <returns></returns>
        private async void SendHeartBeats(object state)
        {
            var cancellationToken= (CancellationToken)state;
            cancellationToken.ThrowIfCancellationRequested();

            var nacosService= serviceProvider.GetService<INacosService>();
            
            var serviceName = GetServiceName(nacosOption.GroupName, nacosOption.ServiceName);

            var beatBody = new SendInstanceHeartBeatInstanceInfoDto()
            {
                Ip = ip,
                Port = nacosOption.Port!.Value,
                ServiceName = serviceName,
                Cluster = "DEFAULT",
                Scheduled = false,
                Weight = nacosOption.Weight,
                MetaData = new Dictionary<string, string>()
                {
                    {"protocol",nacosOption.Protocol}
                }
            };
            try
            {
                var result = await nacosService.SendInstanceHeartBeat(new SendInstanceHeartBeatDto()
                {
                    ServiceName = serviceName,
                    Ephemeral = false,
                    NamespaceId = nacosOption.NamespaceId,
                    GroupName = nacosOption.GroupName,
                    ClusterName = "DEFAULT",
                    Ip = ip,
                    Port = nacosOption.Port!.Value,
                    Beat = beatBody
                });

                logger.LogInformation($"{DateTime.Now},nacos Send Instance HeartBeat,result,{result.ClientBeatInterval}");
                timer.Change(result.ClientBeatInterval, -1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var nacosService = serviceProvider.GetService<INacosService>();

            var serviceName = GetServiceName(nacosOption.GroupName, nacosOption.ServiceName);

            ip = SbUtil.GetCurrentIp();

            var result = await nacosService.RegisterInstance(new NacosRegisterInstanceDto()
            {
                Ip = ip,
                Port = nacosOption.Port!.Value,
                ServiceName = serviceName,
                NamespaceId = nacosOption.NamespaceId,
                GroupName = nacosOption.GroupName,
                ClusterName = "DEFAULT",
                Enabled = true,
                Weight = nacosOption.Weight,
                Ephemeral = true,
                Healthy = true,
                MetaData = new Dictionary<string, string>()
                {
                    {"protocol",nacosOption.Protocol}
                }
            });

            timer = new Timer(SendHeartBeats, cancellationToken, 1000, -1);

            if (result == "ok")
            {
                logger.LogInformation("register instance to nacos server");
            }

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Dispose();

            var nacosService = serviceProvider.GetService<INacosService>();

            var serviceName = GetServiceName(nacosOption.GroupName, nacosOption.ServiceName);

            ip = SbUtil.GetCurrentIp();


            var result = await nacosService.UnRegisterInstance(new NacosRegisterInstanceDto()
            {
                Ip = ip,
                Port = nacosOption.Port!.Value,
                ServiceName = serviceName,
                NamespaceId = nacosOption.NamespaceId,
                GroupName = nacosOption.GroupName,
                ClusterName = "DEFAULT",
                Enabled = true,
                Ephemeral = true,
                Healthy = true,
            });

            if (result == "ok")
            {
                logger.LogInformation("UnRegister Instance To Nacos Server");
            }

            await base.StopAsync(cancellationToken);
        }


        private string GetServiceName(string groupName, string serviceName)
        {
           return groupName + "@@" + serviceName;
        }
    }
}