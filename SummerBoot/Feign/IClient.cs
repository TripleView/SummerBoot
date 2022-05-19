using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SummerBoot.Core;

namespace SummerBoot.Feign
{
    /// <summary>
    /// 实际执行http请求的客户端
    /// </summary>
    public interface IClient
    {
        Task<ResponseTemplate> ExecuteAsync(RequestTemplate requestTemplate, CancellationToken cancellationToken);
        /// <summary>
        /// 默认的IClient类，内部采用httpClient
        /// </summary>
        public class DefaultFeignClient : IClient
        {
            private IHttpClientFactory HttpClientFactory { get; }
            public DefaultFeignClient(IHttpClientFactory iHttpClientFactory)
            {
                HttpClientFactory = iHttpClientFactory;
            }

            public async Task<ResponseTemplate> ExecuteAsync(RequestTemplate requestTemplate, CancellationToken cancellationToken)
            {
                var httpClient = HttpClientFactory.CreateClient(requestTemplate.ClientName);
                
                var httpRequest = new HttpRequestMessage(requestTemplate.HttpMethod, requestTemplate.Url);
                httpRequest.Content = requestTemplate.HttpContent;

                //处理header
                foreach (var requestTemplateHeader in requestTemplate.Headers)
                {
                    //HeaderNames
                    var uppperKey = requestTemplateHeader.Key.ToUpper();

                    var key = uppperKey.Replace("-", "");
                    //判断普通标头
                    if (HttpHeaderSupport.RequestHeaders.Contains(key))
                    {
                        httpRequest.Headers.Remove(requestTemplateHeader.Key);
                        httpRequest.Headers.Add(requestTemplateHeader.Key, requestTemplateHeader.Value);
                    }
                    //判断body标头
                    else if (HttpHeaderSupport.ContentHeaders.Contains(key))
                    {
                        httpRequest.Content.Headers.Remove(requestTemplateHeader.Key);
                        httpRequest.Content.Headers.Add(requestTemplateHeader.Key, requestTemplateHeader.Value);
                    }
                    //自定义标头
                    else
                    {
                        httpRequest.Headers.TryAddWithoutValidation(requestTemplateHeader.Key,
                            requestTemplateHeader.Value);
                    }
                }

                var httpResponse = await httpClient.SendAsync(httpRequest,cancellationToken);
                
                //兼容返回类型不正规的接口，比如nacos
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var message = "";
                    if (httpResponse.Content != null)
                    {
                        message= httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                        
                    if (message.IsNullOrWhiteSpace())
                    {
                        message = httpResponse.ReasonPhrase;
                    }

                    throw new HttpRequestException(message);
                }
                
                //把httpResponseMessage转化为responseTemplate
                var result = await ConvertResponseAsync(httpResponse);

                return result;
            }

            /// <summary>
            /// 把httpResponseMessage转化为responseTemplate
            /// </summary>
            /// <param name="responseMessage"></param>
            /// <returns></returns>
            private async Task<ResponseTemplate> ConvertResponseAsync(HttpResponseMessage responseMessage)
            {
                var responseTemplate = new ResponseTemplate
                {
                    HttpStatusCode = responseMessage.StatusCode,
                    OrignHttpResponseMessage = responseMessage
                };

                var headers = responseMessage.Headers;
                foreach (var httpResponseHeader in headers)
                {
                    responseTemplate.Headers.Add(httpResponseHeader);
                }

                var stream = new MemoryStream();
                await responseMessage.Content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                responseTemplate.Body = stream;
                return responseTemplate;
            }
        }
    }
}
