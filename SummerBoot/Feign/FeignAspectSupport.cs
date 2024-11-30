using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SummerBoot.Feign.Attributes;
using SummerBoot.Feign.Nacos;
using SummerBoot.Feign.Nacos.Dto;

namespace SummerBoot.Feign
{
    public class FeignAspectSupport
    {
        private IServiceProvider _serviceProvider;
        /// <summary>
        /// 解析方法的参数以及值
        /// </summary>
        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        /// <summary>
        /// 解析需要添加到url的参数以及值
        /// </summary>
        private Dictionary<string, object> urlParameters = new Dictionary<string, object>();
        /// <summary>
        /// 编码器
        /// </summary>
        private IFeignEncoder encoder;
        /// <summary>
        /// 解码器
        /// </summary>
        private IFeignDecoder decoder;

        private IFeignUnitOfWork feignUnitOfWork;

        private FeignOption feignOption;
        private IConfiguration configuration;
        public async Task<T> BaseExecuteAsync<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            urlParameters.Clear();
            parameters.Clear();
            _serviceProvider = serviceProvider;
            //获得具体的client客户端
            var feignClient = serviceProvider.GetService<IClient>();

            //序列化器与反序列化器
            encoder = serviceProvider.GetService<IFeignEncoder>();
            decoder = serviceProvider.GetService<IFeignDecoder>();
            feignOption = serviceProvider.GetService<FeignOption>();
            configuration = serviceProvider.GetService<IConfiguration>();
            feignUnitOfWork = serviceProvider.GetService<IFeignUnitOfWork>();
            //读取feignClientAttribute里的信息；
            //接口类型
            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            var feignClientAttribute = interfaceType.GetCustomAttribute<FeignClientAttribute>();
            if (feignClientAttribute == null) throw new Exception(nameof(feignClientAttribute));

            var url = await GetUrlBody(feignClientAttribute);

            var clientName = feignClientAttribute.Name.GetValueOrDefault(interfaceType.FullName);

            var requestPath = url;
            var requestTemplate = new RequestTemplate()
            {
                ClientName = clientName
            };

            //处理参数
            ProcessParameter(method, args, requestTemplate, encoder);

            //处理请求头逻辑
            ProcessHeaders(method, requestTemplate, interfaceType);

            var path = "";

            var mappingCount = 0;
            //是否仅使用路径作为url
            var usePathAsUrl = false;
            //处理get逻辑
            var getMappingAttribute = method.GetCustomAttribute<GetMappingAttribute>();
            if (getMappingAttribute != null)
            {
                mappingCount++;
                path = getMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Get;
                usePathAsUrl = getMappingAttribute.UsePathAsUrl;
            }

            //处理post逻辑
            var postMappingAttribute = method.GetCustomAttribute<PostMappingAttribute>();
            if (postMappingAttribute != null)
            {
                mappingCount++;
                path = postMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Post;
                usePathAsUrl = postMappingAttribute.UsePathAsUrl;
            }

            //处理put逻辑
            var putMappingAttribute = method.GetCustomAttribute<PutMappingAttribute>();
            if (putMappingAttribute != null)
            {
                mappingCount++;
                path = putMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Put;
                usePathAsUrl = putMappingAttribute.UsePathAsUrl;
            }

            //处理Delete逻辑
            var deleteMappingAttribute = method.GetCustomAttribute<DeleteMappingAttribute>();
            if (deleteMappingAttribute != null)
            {
                mappingCount++;
                path = deleteMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Delete;
                usePathAsUrl = deleteMappingAttribute.UsePathAsUrl;
            }

            //处理patch逻辑
            var patchMappingAttribute = method.GetCustomAttribute<PatchMappingAttribute>();
            if (patchMappingAttribute != null)
            {
                mappingCount++;
                path = patchMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Patch;
                usePathAsUrl = patchMappingAttribute.UsePathAsUrl;
            }

            if (mappingCount > 1)
            {
                throw new NotSupportedException("only support one httpMapping");
            }

            path = GetValueByConfiguration(path);

            //如果仅使用path作为url，就不需要添加feignClient上的url前缀
            var urlTemp = usePathAsUrl ? path : CombineUrl(requestPath,path);
            urlTemp = ReplaceVariable(urlTemp);
            urlTemp = AddUrlParameter(urlTemp);
            urlTemp = GetUrl(urlTemp);

            requestTemplate.Url = urlTemp;

            ProcessCookie(requestTemplate);
            //是否忽略拦截器
            var ignoreInterceptorAttribute = method.GetCustomAttribute<IgnoreInterceptorAttribute>();
            if (ignoreInterceptorAttribute == null)
            {
                var hasMethodInterceptor = false;
                if (feignClientAttribute.InterceptorType != null)
                {
                    hasMethodInterceptor = true;
                    //获得请求拦截器
                    var requestInterceptor = (IRequestInterceptor)serviceProvider.GetService(feignClientAttribute.InterceptorType);
                    await requestInterceptor.ApplyAsync(requestTemplate);
                }
                //如果方法上没有拦截器，则使用全局拦截器
                if (!hasMethodInterceptor && feignOption.GlobalInterceptorType != null)
                {
                    //获得请求拦截器
                    var requestInterceptor = (IRequestInterceptor)serviceProvider.GetService(feignOption.GlobalInterceptorType);
                    await requestInterceptor.ApplyAsync(requestTemplate);
                }
            }
            var responseTemplate = await feignClient.ExecuteAsync(requestTemplate, new CancellationToken());
            //直接返回文件流
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)responseTemplate.Body;
            }
            //返回原始信息
            if (typeof(HttpResponseMessage).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)responseTemplate.OrignHttpResponseMessage;
            }

            Exception ex;
            try
            {
                var resultTmp = decoder.Decoder<T>(responseTemplate);

                return resultTmp;
            }
            catch (Exception e)
            {
                ex = e;
            }

            responseTemplate.Body.Seek(0, SeekOrigin.Begin);
            throw new Exception($@"response Decoder error,content is {responseTemplate.Body.ConvertToString()}", ex);
        }

        public string CombineUrl(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/').TrimEnd('/');
            uri2 = uri2.TrimStart('/').TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }

        /// <summary>
        /// 获取url主体部分
        /// </summary>
        /// <param name="feignClientAttribute"></param>
        /// <returns></returns>
        private async Task<string> GetUrlBody(FeignClientAttribute feignClientAttribute)
        {
            var url = feignClientAttribute.Url;
            url = GetValueByConfiguration(url);
            //判断是否为微服务模式
            if (feignOption.EnableNacos && feignClientAttribute.MicroServiceMode)
            {
                var nacosService = _serviceProvider.GetService<INacosService>();
                var serviceName = feignClientAttribute.ServiceName;
                serviceName = GetValueByConfiguration(serviceName);
                if (serviceName.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException("feignClientAttribute's ServiceName can not be null");
                }
                //先从注解上读取
                var namespaceId = feignClientAttribute.NacosNamespaceId;
                
                if (namespaceId.IsNullOrWhiteSpace())
                {
                    //从配置文件中读取
                    namespaceId = configuration.GetSection("nacos:defaultNacosNamespaceId")?.Value;
                }
                if (namespaceId.IsNullOrWhiteSpace())
                {
                    //获得默认值
                    namespaceId = "public";
                }
                namespaceId = GetValueByConfiguration(namespaceId);

                var groupName = feignClientAttribute.NacosGroupName;
                if (groupName.IsNullOrWhiteSpace())
                {
                    groupName = configuration.GetSection("nacos:defaultNacosGroupName")?.Value;
                }
                if (groupName.IsNullOrWhiteSpace())
                {
                    groupName = "DEFAULT_GROUP";
                }
                groupName = GetValueByConfiguration(groupName);

                if (serviceName.HasText())
                {
                    var serviceInstance = await nacosService.QueryInstanceList(new QueryInstanceListInputDto()
                    {
                        ServiceName = serviceName,
                        HealthyOnly = true,
                        NamespaceId = namespaceId,
                        GroupName = groupName
                    });
                    if (serviceInstance != null && serviceInstance.Hosts?.Count > 0)
                    {
                        var lbStrategy = configuration.GetSection("nacos:lbStrategy")?.Value;
                        var tempHosts = new List<QueryInstanceListItemOutputDto>();
                        //根据权重加权后进行随机
                        if (lbStrategy == "WeightRandom")
                        {
                            foreach (var hostDto in serviceInstance.Hosts)
                            {
                                hostDto.Weight = hostDto.Weight == 0 ? 1 : hostDto.Weight;
                                for (int i = 0; i < hostDto.Weight; i++)
                                {
                                    tempHosts.Add(hostDto);
                                }
                            }
                        }
                        else
                        {
                            tempHosts = serviceInstance.Hosts;
                        }
                        //采用随机算法分配要请求的服务器
                        Random random = new Random();
                        int randomPos = random.Next(tempHosts.Count);
                        var host = tempHosts[randomPos];
                        var protocol = "";
                        host.Metadata?.TryGetValue("protocol", out protocol);
                        protocol = protocol.GetValueOrDefault("http");
                        url = $"{protocol}://{host.Ip}:{host.Port}";
                    }
                    else
                    {
                        throw new Exception($"No instance of service named \"{serviceName}\" was found");
                    }
                }
            }

            return url;
        }

        private string GetUrl(string urlTemp)
        {
            Func<string, string> func = (string s) =>
             {
                 s = s.Replace("//", "/");
                 s = s.Replace("///", "/");
                 s = "http://" + s;
                 return s;
             };

            if (urlTemp.Length < 8)
            {
                return func(urlTemp);
            }

            var isHttp = urlTemp.Substring(0, 7) == "http://";
            var isHttps = urlTemp.Substring(0, 8) == "https://";
            if (!isHttp && !isHttps)
            {
                return func(urlTemp);
            }

            if (isHttp)
            {
                urlTemp = urlTemp.Substring(7, urlTemp.Length - 7);
                return func(urlTemp);
            }

            if (isHttps)
            {
                urlTemp = urlTemp.Substring(8, urlTemp.Length - 8);
                urlTemp = urlTemp.Replace("//", "/");
                urlTemp = urlTemp.Replace("///", "/");
                urlTemp = "https://" + urlTemp;
            }

            return urlTemp;
        }

        /// <summary>
        /// 处理cookie
        /// </summary>
        /// <param name="requestTemplate"></param>
        private void ProcessCookie(RequestTemplate requestTemplate)
        {
            if (!feignUnitOfWork.IsShareCookie)
            {
                return;
            }

            var cookies = feignUnitOfWork.GetCookies(requestTemplate.Url);
            var cookieList = new List<string>();
            foreach (Cookie cookie in cookies)
            {
                cookieList.Add($"{cookie.Name}={Uri.EscapeUriString(cookie.Value)}");
            }

            if (cookieList.Any())
            {
                var key = "Cookie";
                var keyValue = string.Join("; ", cookieList);

                var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);

                if (!hasHeaderKey)
                {
                    keyList = new List<string>();
                    keyList.Add(keyValue);
                    requestTemplate.Headers.Add(key, keyList);
                }
                else if (keyList.Count > 0)
                {
                    keyList[0] += $" ;{keyValue}";
                }
            }
        }

        /// <summary>
        /// 处理请求头逻辑
        /// </summary>
        /// <param name="method"></param>
        /// <param name="requestTemplate"></param>
        private void ProcessHeaders(MethodInfo method, RequestTemplate requestTemplate, Type interfaceType)
        {
            var headersAttributes = new List<HeadersAttribute>();
            var methodHeadersAttribute = method.GetCustomAttribute<HeadersAttribute>();
            var interfaceHeadersAttribute = interfaceType.GetCustomAttribute<HeadersAttribute>();
            if (methodHeadersAttribute != null)
            {
                headersAttributes.Add(methodHeadersAttribute);
            }

            if (interfaceHeadersAttribute != null)
            {
                headersAttributes.Add(interfaceHeadersAttribute);
            }

            foreach (var headersAttribute in headersAttributes)
            {
                if (headersAttribute != null)
                {
                    var headerParams = headersAttribute.Param;
                    foreach (var headerParam in headerParams)
                    {
                        if (headerParam.HasIndexOf(':'))
                        {
                            var headerParamArr = headerParam.Split(":");
                            var key = headerParamArr[0].Trim();
                            var keyValue = headerParamArr[1];
                            //替换变量
                            key = ReplaceVariable(key).Trim();
                            keyValue = ReplaceVariable(keyValue).Trim();

                            var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);

                            if (!hasHeaderKey)
                            {
                                keyList = new List<string>();
                                keyList.Add(keyValue);
                                requestTemplate.Headers.Add(key, keyList);
                            }
                            else
                            {
                                keyList.Add(keyValue);
                            }
                        }
                    }
                }
            }
        }

        private string ReplaceVariable(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            str = Regex.Replace(str, "\\{\\{[^\\}]*\\}\\}", match =>
            {
                string matchValue = match.Value;
                foreach (var pair in parameters)
                {
                    if (matchValue.Replace(" ", "") == $"{{{{{pair.Key}}}}}")
                    {
                        return pair.Value.ToString();
                    }
                }

                return "";

            }, RegexOptions.Compiled);

            //foreach (var pair in parameters)
            //{


            //    str = str.Replace($"{{{pair.Key}}}", pair.Value);
            //}

            return str;
        }

        /// <summary>
        /// 通过配置文件获取具体的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetValueByConfiguration(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return value;
            }
            var str = Regex.Replace(value, "\\$\\{[^\\}]*\\}", match =>
            {
                var matchValue = match.Value;
                if (matchValue.Length >= 3)
                {
                    if (matchValue.Substring(0, 2) == "${")
                    {
                        matchValue = matchValue.Substring(2);
                    }
                    if (matchValue.Substring(matchValue.Length - 1, 1) == "}")
                    {
                        matchValue = matchValue.Substring(0, matchValue.Length - 1);
                    }

                    matchValue = matchValue.Trim();
                    matchValue = configuration.GetSection(matchValue).Value;
                    return matchValue;
                }
                return "";

            }, RegexOptions.Compiled);
            return str;
        }

        /// <summary>
        /// 为url添加query参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string AddUrlParameter(string url)
        {
            if (urlParameters.Count == 0)
            {
                return url;
            }

            var valueList = new List<string>();
            foreach (var pair in urlParameters)
            {
                var pairValue = pair.Value.ToString();
                if (pair.Value is Dictionary<string, object>)
                {
                    pairValue = encoder.EncoderObject(pair.Value);
                }

                if (pairValue.IsNullOrEmpty())
                {
                    continue;
                }

                var value = Uri.EscapeDataString(pairValue);
                var urlPair = $"{pair.Key}={value}";
                valueList.Add(urlPair);
            }
            var urlString = string.Join("&", valueList.ToArray());

            var reg = new Regex(".*\\?.*=.*");

            if (reg.IsMatch(url))
            {
                return url + "&" + urlString;
            }

            return url + "?" + urlString;
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        private void ProcessParameter(MethodInfo method, object[] args, RequestTemplate requestTemplate, IFeignEncoder encoder)
        {
            var parameterInfos = method.GetParameters();
            var multipartAttribute = method.GetCustomAttribute<MultipartAttribute>();
            var multipartFormDataContent = new MultipartFormDataContent();


            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var arg = args[i];
                var parameterInfo = parameterInfos[i];
                var parameterType = parameterInfos[i].ParameterType;
                var aliasAsAttribute = parameterInfo.GetCustomAttribute<AliasAsAttribute>();
                var bodyAttribute = parameterInfo.GetCustomAttribute<BodyAttribute>();
                var queryAttribute = parameterInfo.GetCustomAttribute<QueryAttribute>();
                var parameterName = aliasAsAttribute != null ? aliasAsAttribute.Name : parameterInfos[i].Name;
                var isEmbedded = parameterInfo.GetCustomAttribute<EmbeddedAttribute>() != null;
                //处理body类型
                if (bodyAttribute != null)
                {
                    switch (bodyAttribute.SerializationKind)
                    {
                        case BodySerializationKind.Json:
                            requestTemplate.HttpContent = encoder.Encoder(args[i]);

                            break;
                        case BodySerializationKind.Form:

                            if (parameterType.IsString())
                            {
                                requestTemplate.HttpContent = new StringContent(Uri.EscapeDataString(arg.ToString()),
                                    Encoding.UTF8, "application/x-www-form-urlencoded");
                            }
                            else
                            {
                                var dictionary = new Dictionary<string, object>();
                                if (parameterType.IsClass)
                                {
                                    var keyValue = AddClassParameter(parameterType, arg, false, false);
                                    foreach (var keyValuePair in keyValue)
                                    {
                                        dictionary.AddIfNotExist(keyValuePair.Key, keyValuePair.Value);
                                    }
                                }
                                else
                                {
                                    throw new NotSupportedException(
                                        "bodyAttribute is not use for:" + parameterType.Name);
                                }

                                if (multipartAttribute == null)
                                {
                                    var newDictionary = new Dictionary<string, string>();
                                    foreach (var keyValuePair in dictionary)
                                    {
                                        newDictionary.Add(keyValuePair.Key, keyValuePair.Value.ToString());

                                    }
                                    requestTemplate.HttpContent = new FormUrlEncodedContent(newDictionary);
                                }
                                else
                                {
                                    foreach (KeyValuePair<string, object> pair in dictionary)
                                    {
                                        multipartFormDataContent.Add(new StringContent(pair.Value.ToString()), pair.Key);
                                    }
                                }

                            }

                            break;
                    }
                }
                //处理query类型
                else if (queryAttribute != null)
                {
                    if (arg != null)
                    {
                        if (parameterType.IsString() || parameterType.IsValueType)
                        {
                            urlParameters.AddIfNotExist(parameterName, arg.ToString());
                        }
                        else if (parameterType.IsClass)
                        {
                            var keyValue = AddClassParameter(parameterType, arg, isEmbedded, true);
                            if (isEmbedded)
                            {
                                urlParameters.AddIfNotExist(parameterName, keyValue);
                            }
                            else
                            {
                                foreach (var keyValuePair in keyValue)
                                {
                                    urlParameters.AddIfNotExist(keyValuePair.Key, keyValuePair.Value);
                                }
                            }
                        }
                    }
                }
                //处理普通的参数
                else
                {
                    if (arg != null)
                    {
                        if (parameterType.IsString() || parameterType.IsValueType)
                        {
                            parameters.AddIfNotExist(parameterName, arg.ToString());
                        }
                        else if (arg is BasicAuthorization baseAuthorization)
                        {
                            var baseAuthString = baseAuthorization.GetBaseAuthString();
                            if (baseAuthString.HasText())
                            {
                                var key = HeaderNames.Authorization.ToUpper();
                                var keyValue = baseAuthString;
                                var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);

                                if (!hasHeaderKey)
                                {
                                    keyList = new List<string>();
                                    keyList.Add(keyValue);
                                    requestTemplate.Headers.Add(HeaderNames.Authorization, keyList);
                                }
                                else
                                {
                                    keyList.Add(keyValue);
                                }
                            }
                        }
                        else if (arg is HeaderCollection headerCollection)
                        {
                            foreach (var keyValuePair in headerCollection)
                            {
                                var key = keyValuePair.Key;
                                var keyValue = keyValuePair.Value;

                                var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);

                                if (!hasHeaderKey)
                                {
                                    keyList = new List<string>();
                                    keyList.Add(keyValue);
                                    requestTemplate.Headers.Add(key, keyList);
                                }
                                else
                                {
                                    keyList.Add(keyValue);
                                }
                            }

                        }
                        else if (arg is CookieCollection cookieCollection)
                        {
                            var cookieList = new List<string>();
                            foreach (Cookie cookie in cookieCollection)
                            {
                                cookieList.Add($"{cookie.Name}={Uri.EscapeUriString(cookie.Value)}");
                            }

                            if (cookieList.Any())
                            {
                                var key = "Cookie";
                                var keyValue = string.Join("; ", cookieList);

                                var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);

                                if (!hasHeaderKey)
                                {
                                    keyList = new List<string>();
                                    keyList.Add(keyValue);
                                    requestTemplate.Headers.Add(key, keyList);
                                }
                                else
                                {
                                    keyList.Add(keyValue);
                                }
                            }
                        }
                        else if (arg is MultipartItem multipartItem)
                        {

                            var itemName = aliasAsAttribute != null ? aliasAsAttribute.Name : multipartItem.Name;
                            multipartFormDataContent.Add(multipartItem.Content, itemName, multipartItem.FileName);
                            if (multipartAttribute == null)
                            {
                                requestTemplate.HttpContent = multipartFormDataContent;
                            }
                        }
                        else if (parameterType.IsClass)
                        {
                            var keyValue = AddClassParameter(parameterType, arg, false, false);
                            foreach (var keyValuePair in keyValue)
                            {
                                parameters.AddIfNotExist(keyValuePair.Key, keyValuePair.Value);
                            }
                        }

                    }
                }
            }

            if (multipartAttribute != null)
            {
                requestTemplate.HttpContent = multipartFormDataContent;
            }
        }

        /// <summary>
        /// 添加类类型的参数
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="arg">参数</param>
        /// <param name="originDictionary"></param>
        /// <param name="originEmbedded">初始是否嵌套</param>
        /// <param name="totalEmbedded">是否嵌套总开关，总开关关闭则全体不嵌套</param>
        /// <returns></returns>
        private Dictionary<string, object> AddClassParameter(Type parameterType, object arg, bool originEmbedded, bool totalEmbedded)
        {
            if (!totalEmbedded)
            {
                originEmbedded = false;
            }
            //正常返回值
            var targetDictionary = new Dictionary<string, object>();
            //如果是字典类型
            if (parameterType.IsDictionary())
            {
                if (arg is IDictionary argDictionary)
                {
                    foreach (DictionaryEntry keyValue in argDictionary)
                    {
                        var key = keyValue.Key.ToString();
                        var value = keyValue.Value?.ToString();
                        targetDictionary.AddIfNotExist(key, value);
                    }
                }
            }
            //正常类
            else
            {
                var parameterPropertyInfos = parameterType.GetProperties();
                foreach (var propertyInfo in parameterPropertyInfos)
                {
                    //判断是否序列化
                    var isEmbedded = (propertyInfo.GetCustomAttribute<EmbeddedAttribute>() != null || originEmbedded) && totalEmbedded;

                    var propertyAliasAsAttribute = propertyInfo.GetCustomAttribute<AliasAsAttribute>();

                    var key = propertyAliasAsAttribute != null
                        ? propertyAliasAsAttribute.Name
                        : propertyInfo.Name;
                    //处理常规类型
                    var value = propertyInfo.GetValue(arg);
                    if (propertyInfo.PropertyType.IsString() || propertyInfo.PropertyType.IsValueType)
                    {
                        if (value != null)
                        {
                            targetDictionary.AddIfNotExist(key, value.ToString());
                        }
                    }
                    else
                    {
                        var keyValue = AddClassParameter(propertyInfo.PropertyType, value, isEmbedded, totalEmbedded);

                        if (isEmbedded)
                        {
                            targetDictionary.AddIfNotExist(key, keyValue);
                        }
                        else
                        {
                            foreach (var keyValuePair in keyValue)
                            {
                                targetDictionary.AddIfNotExist(keyValuePair.Key, keyValuePair.Value);
                            }
                        }
                    }
                }
            }

            return targetDictionary;
        }
    }
}
