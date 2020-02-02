using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                foreach (var requestTemplateHeader in requestTemplate.Headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(requestTemplateHeader.Key, requestTemplateHeader.Value);
                }

                //httpClient.BaseAddress = new Uri(requestTemplate.Url);
                var httpResponse = new HttpResponseMessage();
                if (requestTemplate.HttpMethod == HttpMethod.Get)
                {
                    httpResponse = await httpClient.GetAsync(requestTemplate.Url, cancellationToken);
                }
                else if (requestTemplate.HttpMethod == HttpMethod.Post)
                {
                    var content = new StringContent(requestTemplate.Body);
                    httpResponse = await httpClient.PostAsync(requestTemplate.Url, content, cancellationToken);
                }

                //把httpResponseMessage转化为responseTemplate
                var result= await ConvertResponseAsync(httpResponse);

                return result;
            }

            private async Task<ResponseTemplate> ConvertResponseAsync( HttpResponseMessage responseMessage)
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
