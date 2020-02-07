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
    public interface IClient
    {
        Task<ResponseTemplate> ExecuteAsync(RequestTemplate requestTemplate, CancellationToken cancellationToken);

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

                foreach (var requestTemplateHeader in requestTemplate.Headers)
                {
                    var uppperKey = requestTemplateHeader.Key.ToUpper();
                    var key = uppperKey.Replace("-", "");
                    if (HttpHeaderSupport.RequestHeaders.Contains(key))
                    {
                        httpRequest.Headers.Remove(requestTemplateHeader.Key);
                        httpRequest.Headers.Add(requestTemplateHeader.Key, requestTemplateHeader.Value);
                    }
                }

                if (requestTemplate.HttpMethod == HttpMethod.Post)
                {
                    var content = new StringContent(requestTemplate.Body);
                    foreach (var header in requestTemplate.Headers)
                    {
                        var uppperKey = header.Key.ToUpper();
                        var key = uppperKey.Replace("-", "");
                        if (HttpHeaderSupport.ContentHeaders.Contains(key))
                        {
                            content.Headers.Remove(header.Key);
                            content.Headers.Add(header.Key, header.Value);
                        }
                    }

                    httpRequest.Content = content;
                }

                //var temp = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestTemplate.Url));
                var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);

                //把httpResponseMessage转化为responseTemplate
                var result = await ConvertResponseAsync(httpResponse);

                return result;
            }

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
