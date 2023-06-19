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

        private SemaphoreSlim semaphoreSlim ;
        //private string lastContent;

        public NacosConfigurationProvider(IConfiguration configuration)
        {
            //lastContent = "";
            this.configuration = configuration;
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddLogging();
            serviceCollection.AddSummerBoot();
            serviceCollection.AddSummerBootFeign(it => it.AddNacos(configuration));
            serviceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;
            nacosOption = configuration.GetSection("nacos").Get<NacosOption>();
            lastContentDic = new Dictionary<string, string>();
            if (nacosOption == null || nacosOption.ConfigurationOption == null || nacosOption.ConfigurationOption.Count==0)
            {
                throw new ArgumentException("nacos ConfigurationOption can not be null");
            }

            semaphoreSlim = new SemaphoreSlim(1);

            foreach (var nacosConfigurationOption in nacosOption.ConfigurationOption)
            {
                var key = nacosConfigurationOption.NamespaceId + "-" + nacosConfigurationOption.DataID + "-" +
                          nacosConfigurationOption.GroupName;
                lastContentDic[key] = "";

                if (nacosConfigurationOption.DataID.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("ConfigurationOption.DataId can not be empty");
                }
                if (nacosConfigurationOption.NamespaceId.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("ConfigurationOption.NamespaceId can not be empty");
                }
            }

            if (nacosOption.NamespaceId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("NamespaceId can not be empty");
            }

        }

        public override void Load()
        {
            Console.Write("进来了");
            //GetConfigInternal(new CancellationToken()).GetAwaiter().GetResult();
            timer = new Timer(GetConfig, new CancellationToken(), 0, -1);
        }

        private async void GetConfig(object state)
        {
            
            try
            {
                await semaphoreSlim.WaitAsync(TimeSpan.FromMinutes(300));
                await GetConfigInternal(state);
            }
            finally
            {
                semaphoreSlim.Release();
                //timer?.Change(0, -1);
            }
          
        }

        private Dictionary<string, string> lastContentDic;

        private async Task GetConfigInternal(object state)
        {
            var cancellationToken = (CancellationToken)state;
            cancellationToken.ThrowIfCancellationRequested();

            var nacosService = serviceProvider.GetRequiredService<INacosService>();

            var logger = serviceProvider.GetRequiredService<ILogger<NacosConfigurationProvider>>();

            try
            {
                var allDic = new Dictionary<string, string>();
                foreach (var nacosConfigurationOption in nacosOption.ConfigurationOption)
                {
                    var key = nacosConfigurationOption.NamespaceId + "-" + nacosConfigurationOption.DataID + "-" +
                              nacosConfigurationOption.GroupName;
                    var lastContent = lastContentDic[key];
                    var lastContentMd5 = lastContent.IsNullOrWhiteSpace() ? string.Empty : lastContent.ToMd5();
                    var sb = new StringBuilder();
                    sb.Append(nacosConfigurationOption.DataID)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(nacosConfigurationOption.GroupName)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(lastContentMd5)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(nacosConfigurationOption.NamespaceId)
                        .Append(NacosUtil.LINE_SEPARATOR);
                    
                    var param = sb.ToString();
                    var configListenerResult = await nacosService.ConfigListener(param);

                    if (nacosConfigurationOption.NamespaceId == "public" || (nacosConfigurationOption.NamespaceId != "public" && configListenerResult.HasText()))
                    {
                        var httpResponseMessage = await nacosService.GetConfigs(new GetConfigsDto()
                        {
                            DataId = nacosConfigurationOption.DataID,
                            Group = nacosConfigurationOption.GroupName,
                            NameSpaceId = nacosConfigurationOption.NamespaceId
                        });

                        httpResponseMessage.EnsureSuccessStatusCode();

                        var result = await httpResponseMessage.Content.ReadAsStringAsync();
                        lastContentDic[key] = result;

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

                                allDic.AddRange(jsonParseResult);
                                break;
                            case "xml":
                                var xmlParseResult = XmlParser.Parse(result);
                                allDic.AddRange(xmlParseResult);
                                break;
                            case "yaml":
                                var yamlParseResult = YamlParser.Parse(result);
                                allDic.AddRange(yamlParseResult);
                                break;
                        }

                       
                    }
                }

                Data = allDic;
                base.OnReload();
                //logger.LogInformation($"{DateTime.Now},nacos Send Instance HeartBeat,result,{result.ClientBeatInterval}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "get configs error.reason");
            }
            finally
            {
               
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