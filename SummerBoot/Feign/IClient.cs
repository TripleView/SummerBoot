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
                var httpClient = HttpClientFactory.CreateClient();

                var httpRequest = new HttpRequestMessage(requestTemplate.HttpMethod, requestTemplate.Url);

                if (requestTemplate.HttpMethod == HttpMethod.Post)
                {
                    httpRequest.Content = new StringContent(requestTemplate.Body);
                }

                //处理header
                foreach (var requestTemplateHeader in requestTemplate.Headers)
                {
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

                var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);

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
                    HttpStatusCode = responseMessage.StatusCode
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
