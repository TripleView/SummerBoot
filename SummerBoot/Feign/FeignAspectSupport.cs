using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Newtonsoft.Json;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign
{
    public class FeignAspectSupport
    {
        private IServiceProvider _serviceProvider;
        /// <summary>
        /// 解析方法的参数以及值
        /// </summary>
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        public async Task<T> BaseExecuteAsync<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //获得具体的client客户端
            var feignClient = serviceProvider.GetService<IClient>();
            //序列化器与反序列化器
            var encoder = serviceProvider.GetService<IFeignEncoder>();
            var decoder = serviceProvider.GetService<IFeignDecoder>();

            //读取feignClientAttribute里的信息；
            //接口类型
            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            var feignClientAttribute = interfaceType.GetCustomAttribute<FeignClientAttribute>();
            if (feignClientAttribute == null) throw new Exception(nameof(feignClientAttribute));

            var url = feignClientAttribute.Url;

            var clientName = feignClientAttribute.Name.GetValueOrDefault(interfaceType.FullName);
            var requestPath = url;
            var requestTemplate = new RequestTemplate()
            {
                ClientName = clientName
            };

            //获得请求拦截器
            var requestInterceptor = serviceProvider.GetService<IRequestInterceptor>();

            //处理参数
            ProcessParameter(method, args, requestTemplate, encoder);

            //处理请求头逻辑
            ProcessHeaders(method, requestTemplate);

            var path = "";

            var mappingCount = 0;
            //处理get逻辑
            var getMappingAttribute = method.GetCustomAttribute<GetMappingAttribute>();
            if (getMappingAttribute != null)
            {
                mappingCount++;
                path = getMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Get;
            }

            //处理post逻辑
            var postMappingAttribute = method.GetCustomAttribute<PostMappingAttribute>();
            if (postMappingAttribute != null)
            {
                mappingCount++;
                path = postMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Post;
            }

            //处理put逻辑
            var putMappingAttribute = method.GetCustomAttribute<PutMappingAttribute>();
            if (putMappingAttribute != null)
            {
                mappingCount++;
                path = putMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Put;
            }

            //处理Delete逻辑
            var deleteMappingAttribute = method.GetCustomAttribute<DeleteMappingAttribute>();
            if (deleteMappingAttribute != null)
            {
                mappingCount++;
                path = deleteMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Delete;
            }

            if (mappingCount > 1)
            {
                throw new NotSupportedException("only support one httpMapping");
            }

            var urlTemp = requestPath + path;

            urlTemp = GetUrl(urlTemp);

            requestTemplate.Url = ReplaceVariable(urlTemp);

            //如果存在拦截器，则进行拦截
            if (requestInterceptor != null) requestInterceptor.Apply(requestTemplate);

            var responseTemplate = await feignClient.ExecuteAsync(requestTemplate, new CancellationToken());

            //判断方法返回值是否为异步类型
            var isAsyncReturnType = method.ReturnType.IsAsyncType();
            //返回类型
            var returnType = isAsyncReturnType ? method.ReturnType.GenericTypeArguments.First() : method.ReturnType;

            var resultTmp = (T)decoder.Decoder(responseTemplate, returnType);

            return resultTmp;
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
        /// 处理请求头逻辑
        /// </summary>
        /// <param name="method"></param>
        /// <param name="requestTemplate"></param>
        private void ProcessHeaders(MethodInfo method, RequestTemplate requestTemplate)
        {
            var headersAttribute = method.GetCustomAttribute<HeadersAttribute>();
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
                        key = ReplaceVariable(key);
                        keyValue = ReplaceVariable(keyValue);

                        var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);
                        if (!hasHeaderKey) keyList = new List<string>();
                        keyList.Add(keyValue.Trim());
                        if (!hasHeaderKey) requestTemplate.Headers.Add(key, keyList);
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

            foreach (var pair in parameters)
            {
                str = str.Replace($"{{{pair.Key}}}", pair.Value);
            }

            return str;
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        private void ProcessParameter(MethodInfo method, object[] args, RequestTemplate requestTemplate, IFeignEncoder encoder)
        {
            var parameterInfos = method.GetParameters();

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var arg = args[i];
                var parameterInfo = parameterInfos[i];
                var parameterType = parameterInfos[i].ParameterType;
                var aliasAsAttribute = parameterInfo.GetCustomAttribute<AliasAsAttribute>();
                var bodyAttribute = parameterInfo.GetCustomAttribute<BodyAttribute>();

                var parameterName = aliasAsAttribute != null ? aliasAsAttribute.Name : parameterInfos[i].Name;

                //处理body类型
                if (bodyAttribute != null)
                {
                    switch (bodyAttribute.SerializationKind)
                    {
                        case BodySerializationKind.Json:
                            requestTemplate.HttpContent = encoder.Encoder(args[i]);

                            break;
                        case BodySerializationKind.Form:

                            if (arg is string str)
                            {
                                requestTemplate.HttpContent = new StringContent(Uri.EscapeDataString(str),
                                    Encoding.UTF8, "application/x-www-form-urlencoded");
                            }
                            else
                            {
                                var dictionary = new Dictionary<string, string>();

                                if (arg is IDictionary tempDictionary)
                                {
                                    foreach (var key in tempDictionary.Keys)
                                    {
                                        var value = tempDictionary[key];
                                        if (value != null)
                                        {
                                            dictionary.Add(key.ToString(), value?.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    var parameterPropertyInfos = parameterType.GetProperties();
                                    foreach (var propertyInfo in parameterPropertyInfos)
                                    {
                                        var propertyAliasAsAttribute = propertyInfo.GetCustomAttribute<AliasAsAttribute>();

                                        var key = propertyAliasAsAttribute != null
                                            ? propertyAliasAsAttribute.Name
                                            : propertyInfo.Name;

                                        var value = propertyInfo.GetValue(arg);
                                        if (value != null)
                                        {
                                            dictionary.Add(key.ToString(), value?.ToString());
                                        }
                                    }
                                }
                                requestTemplate.HttpContent = new FormUrlEncodedContent(dictionary);
                            }

                            break;
                    }

                }
                else
                {
                    if (arg != null)
                    {
                        if (arg is string str || parameterType.IsValueType)
                        {
                            parameters.Add(parameterName, arg.ToString());
                        }
                        else if (parameterType.IsClass)
                        {
                            var parameterPropertyInfos = parameterType.GetProperties();
                            foreach (var propertyInfo in parameterPropertyInfos)
                            {
                                var propertyAliasAsAttribute = propertyInfo.GetCustomAttribute<AliasAsAttribute>();

                                var key = propertyAliasAsAttribute != null
                                    ? propertyAliasAsAttribute.Name
                                    : propertyInfo.Name;

                                var value = propertyInfo.GetValue(arg);
                                if (value != null)
                                {
                                    parameters.Add(key, value.ToString());
                                }
                            }
                        }

                    }
                }
            }

        }
    }
}
