using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using SummerBoot.Feign.Nacos.Dto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Core.Configuration.Parser;

namespace SummerBoot.Feign.Nacos
{
    public class NacosConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private Timer timer;
        private IServiceProvider serviceProvider;
        private IConfiguration configuration;
        private NacosOption nacosOption;
        private string lastContent;

        public NacosConfigurationProvider(IConfiguration configuration)
        {
            lastContent = "";
            this.configuration = configuration;
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddLogging();
            serviceCollection.AddSummerBoot();
            serviceCollection.AddSummerBootFeign(it => it.AddNacos(configuration));
            serviceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;
            nacosOption = configuration.GetSection("nacos").Get<NacosOption>();
            if (nacosOption == null || nacosOption.ConfigurationOption == null)
            {
                throw new ArgumentException("nacos ConfigurationOption can not be null");
            }

            if (nacosOption.ConfigurationOption.DataID.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("ConfigurationOption.DataId can not be empty");
            }
            if (nacosOption.NamespaceId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("NamespaceId can not be empty");
            }

        }

        public override void Load()
        {
            GetConfigInternal(new CancellationToken()).GetAwaiter().GetResult();
            timer = new Timer(GetConfig, new CancellationToken(), 0, -1);
        }

        private async void GetConfig(object state)
        {
            await GetConfigInternal(state);
        }

        private async Task GetConfigInternal(object state)
        {
            var cancellationToken = (CancellationToken)state;
            cancellationToken.ThrowIfCancellationRequested();

            var nacosService = serviceProvider.GetRequiredService<INacosService>();

            var logger = serviceProvider.GetRequiredService<ILogger<NacosConfigurationProvider>>();

            try
            {
                var lastContentMd5 = lastContent.IsNullOrWhiteSpace() ? string.Empty : lastContent.ToMd5();
                var sb = new StringBuilder();
                sb.Append(nacosOption.ConfigurationOption.DataID)
                    .Append(NacosUtil.WORD_SEPARATOR)
                    .Append(nacosOption.ConfigurationOption.GroupName)
                    .Append(NacosUtil.WORD_SEPARATOR)
                    .Append(lastContentMd5)
                    .Append(NacosUtil.WORD_SEPARATOR)
                    .Append(nacosOption.NamespaceId)
                    .Append(NacosUtil.LINE_SEPARATOR);
                ;
                var param = sb.ToString();
                var configListenerResult = await nacosService.ConfigListener(param);

                if (configListenerResult.HasText())
                {
                    var httpResponseMessage = await nacosService.GetConfigs(new GetConfigsDto()
                    {
                        DataId = nacosOption.ConfigurationOption.DataID,
                        Group = nacosOption.ConfigurationOption.GroupName,
                        NameSpaceId = nacosOption.NamespaceId
                    });

                    httpResponseMessage.EnsureSuccessStatusCode();

                    var result = await httpResponseMessage.Content.ReadAsStringAsync();
                    lastContent = result;

                    var hasConfigType = httpResponseMessage.Headers.TryGetValues(NacosUtil.CONFIG_TYPE, out var listValues);
                    //根据返回的配置类型解析数据
                    //配置类型
                    var configType = "text";
                    var configTypeValues = listValues?.ToList() ?? new List<string>();
                    if (hasConfigType && configTypeValues.Count > 0)
                    {
                        configType = configTypeValues[0];
                    }

                    switch (configType)
                    {
                        case "json":
                            var jsonParseResult = JsonParser.Parse(result);
                            Data = jsonParseResult;
                            break;
                        case "xml":
                            var xmlParseResult = XmlParser.Parse(result);
                            Data = xmlParseResult;
                            break;
                        case "yaml":
                            var yamlParseResult = YamlParser.Parse(result);
                            Data = yamlParseResult;
                            break;
                    }

                    base.OnReload();
                }
                //logger.LogInformation($"{DateTime.Now},nacos Send Instance HeartBeat,result,{result.ClientBeatInterval}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "get configs error");
            }
            finally
            {
                timer?.Change(0, -1);
            }
        }

        ~NacosConfigurationProvider()
        {
            timer.Dispose();
        }
        public void Dispose()
        {
            
        }
    }
}