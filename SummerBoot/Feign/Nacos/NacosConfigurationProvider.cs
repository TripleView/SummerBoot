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
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Feign.Nacos
{
    public class NacosConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private Timer timer;
        private IServiceProvider serviceProvider;
        private IConfiguration configuration;
        private NacosOption nacosOption;

        private SemaphoreSlim semaphoreSlim;
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
            if (nacosOption == null || nacosOption.ConfigurationOption == null || nacosOption.ConfigurationOption.Count == 0)
            {
                throw new ArgumentException("nacos ConfigurationOption can not be null");
            }

            semaphoreSlim = new SemaphoreSlim(1);
            lastDic = new Dictionary<string, IDictionary<string, string>>();

            foreach (var nacosConfigurationOption in nacosOption.ConfigurationOption)
            {
                var namespaceId = nacosConfigurationOption.NamespaceId == "public"
                    ? ""
                    : nacosConfigurationOption.NamespaceId;
                var key = namespaceId + "-" + nacosConfigurationOption.DataID + "-" +
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
                if (nacosConfigurationOption.GroupName.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("ConfigurationOption.GroupName can not be empty");
                }
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

            try
            {
                await semaphoreSlim.WaitAsync();
                await GetConfigInternal(state);
            }
            finally
            {
                semaphoreSlim.Release();
                timer?.Change(0, -1);
            }

        }

        /// <summary>
        /// 存每一个配置的最后值
        /// </summary>
        private Dictionary<string, string> lastContentDic;
        /// <summary>
        /// 存每一个配置的字典的合集
        /// </summary>
        private Dictionary<string, IDictionary<string, string>> lastDic;

        private async Task GetConfigInternal(object state)
        {
            var cancellationToken = (CancellationToken)state;
            cancellationToken.ThrowIfCancellationRequested();

            var nacosService = serviceProvider.GetRequiredService<INacosService>();

            var logger = serviceProvider.GetRequiredService<ILogger<NacosConfigurationProvider>>();

            try
            {
                var allDic = new Dictionary<string, string>();
                var sb = new StringBuilder();
                foreach (var nacosConfigurationOption in nacosOption.ConfigurationOption)
                {
                    var namespaceId = nacosConfigurationOption.NamespaceId == "public"
                        ? ""
                        : nacosConfigurationOption.NamespaceId;

                    var key = namespaceId + "-" + nacosConfigurationOption.DataID + "-" +
                              nacosConfigurationOption.GroupName;
                    var lastContent = lastContentDic[key];
                    var lastContentMd5 = lastContent.IsNullOrWhiteSpace() ? string.Empty : lastContent.ToMd5();

                    sb.Append(nacosConfigurationOption.DataID)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(nacosConfigurationOption.GroupName)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(lastContentMd5)
                        .Append(NacosUtil.WORD_SEPARATOR)
                        .Append(namespaceId)
                        .Append(NacosUtil.LINE_SEPARATOR);
                }
                var param = sb.ToString();

                var configListenerResult = await nacosService.ConfigListener(param);
                if (configListenerResult.HasText())
                {
                    var changeInfos = configListenerResult.Split("%01").Where(it => it.Contains("%02")).ToList();
                    if (changeInfos.Any())
                    {
                        foreach (var changeInfo in changeInfos)
                        {
                            var dataId = "";
                            var groupName = "";
                            var namespaceId = "";

                            var nameSpaceDataIdGroupArr = changeInfo.Split("%02").ToList();
                            if (nameSpaceDataIdGroupArr.Count >= 2)
                            {
                                dataId = nameSpaceDataIdGroupArr[0];
                                groupName = nameSpaceDataIdGroupArr[1];
                            }
                            if (nameSpaceDataIdGroupArr.Count == 3)
                            {
                                namespaceId = nameSpaceDataIdGroupArr[2];
                            }

                            var key = namespaceId + "-" + dataId + "-" +
                                      groupName;

                            var httpResponseMessage = await nacosService.GetConfigs(new GetConfigsDto()
                            {
                                DataId = dataId,
                                Group = groupName,
                                NameSpaceId = namespaceId
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
                                    lastDic[key] = jsonParseResult;

                                    break;
                                case "xml":
                                    var xmlParseResult = XmlParser.Parse(result);
                                    lastDic[key] = xmlParseResult;
                                    break;
                                case "yaml":
                                    var yamlParseResult = YamlParser.Parse(result);
                                    lastDic[key] = yamlParseResult;
                                    break;
                            }
                        }

                        foreach (var nacosConfigurationOption in nacosOption.ConfigurationOption)
                        {
                            var namespaceId = nacosConfigurationOption.NamespaceId == "public"
                                ? ""
                                : nacosConfigurationOption.NamespaceId;

                            var key = namespaceId + "-" + nacosConfigurationOption.DataID + "-" +
                                      nacosConfigurationOption.GroupName;
                            if (lastDic.ContainsKey(key))
                            {
                                allDic.AddRange(lastDic[key]);
                            }
                        }

                        Data = allDic;
                        base.OnReload();
                    }
                }

                //logger.LogInformation($"{DateTime.Now},nacos Send Instance HeartBeat,result,{result.ClientBeatInterval}");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to obtain nacos configuration, reason:" + e.Message);
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