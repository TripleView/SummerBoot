using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SummerBoot.Cache;
using SummerBoot.Core;
using Microsoft.Extensions.DependencyInjection;

namespace SummerBoot.Feign
{
    public class FeignAspectSupport
    {
        private IServiceProvider _serviceProvider;

        public async Task<T> BaseExecuteAsync<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //获得具体的client客户端
            var feignClient = serviceProvider.GetService<IClient>();
            //序列化器与反序列化器
            var encoder = serviceProvider.GetService<IEncoder>();
            var decoder = serviceProvider.GetService<IDecoder>();

            //读取feignClientAttribute里的信息；
            //接口类型
            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));
            var feignClientAttribute = interfaceType.GetCustomAttribute<FeignClientAttribute>();
            var url = feignClientAttribute.Url;
            var path = feignClientAttribute.Path;
            var path2 = string.Empty;
            var clientName = feignClientAttribute.Name;
            var requestPath = url + path;
            var requestTemplate = new RequestTemplate();

            //处理请求头逻辑
            ProcessHeaders(method, requestTemplate);

            //处理get逻辑
            var getMappingAttribute = method.GetCustomAttribute<GetMappingAttribute>();
            if (getMappingAttribute != null)
            {
                path2 = getMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Get;
            }

            //处理post逻辑
            var postMappingAttribute = method.GetCustomAttribute<PostMappingAttribute>();
            if (postMappingAttribute != null)
            {
                path2 = postMappingAttribute.Value;
                requestTemplate.HttpMethod = HttpMethod.Post;
            }

            requestTemplate.Url = requestPath + path2;

            //处理参数,因为有些参数需要拼接到url里，所以要在url处理完毕后才能处理参数
            ProcessParameter(method, args, requestTemplate, encoder);

            var responseTemplate = await feignClient.ExecuteAsync(requestTemplate, new CancellationToken());

            //判断方法返回值是否为异步类型
            var isAsyncReturnType = method.ReturnType.IsAsyncType();
            //返回类型
            var returnType = isAsyncReturnType ? method.ReturnType.GenericTypeArguments.First() : method.ReturnType;

            var resultTmp = (T)decoder.Decoder(responseTemplate, returnType);

            return resultTmp;
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
                        var hasHeaderKey = requestTemplate.Headers.TryGetValue(key, out var keyList);
                        if (!hasHeaderKey) keyList = new List<string>();
                        keyList.Add(keyValue.Trim());
                        if (!hasHeaderKey) requestTemplate.Headers.Add(key, keyList);
                    }
                }
            }
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        private void ProcessParameter(MethodInfo method, object[] args, RequestTemplate requestTemplate, IEncoder encoder)
        {
            var parameterInfos = method.GetParameters();
            //所有参数里，只能有一个body的注解
            var hasBodyAttribute = false;
            //参数集合
            var parameters = new Dictionary<string, string>();
            //url
            var url = requestTemplate.Url;

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var arg = args[i];
                var parameterInfo = parameterInfos[i];
                var parameterType = parameterInfos[i].ParameterType;
                var paramAttribute = parameterInfo.GetCustomAttribute<ParamAttribute>();
                var bodyAttribute = parameterInfo.GetCustomAttribute<BodyAttribute>();
                var parameterName = parameterInfos[i].Name;

                if (paramAttribute != null && bodyAttribute != null)
                {
                    throw new Exception(parameterType.Name + "can not accept parameterAttrite and bodyAttribute");
                }

                if (hasBodyAttribute) throw new Exception("bodyAttribute just only one");
                if (bodyAttribute != null) hasBodyAttribute = true;

                var parameterTypeIsString = parameterType.IsString();

                //处理param类型
                if ((parameterTypeIsString || parameterType.IsValueType) && paramAttribute != null)
                {
                    parameterName = paramAttribute.Value.GetValueOrDefault(parameterName);
                    parameters.Add(parameterName, arg.ToString());
                }

                //处理body类型
                if (!parameterTypeIsString && parameterType.IsClass && bodyAttribute != null)
                {
                    encoder.Encoder(args[i], requestTemplate);
                }
            }

            var strParam = string.Join("&", parameters.Select(o => o.Key + "=" + o.Value));
            if (strParam.HasText()) url = string.Concat(url, '?', strParam);
            requestTemplate.Url = url;
        }
    }
}
