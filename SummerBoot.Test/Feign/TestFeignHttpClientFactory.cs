using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using SummerBoot.Feign;
using Xunit;

namespace SummerBoot.Test.Feign
{
    public class TestFeignHttpClientFactory : IHttpClientFactory
    {
        private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;

        private readonly IServiceProvider serviceProvider;

        public TestFeignHttpClientFactory(IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor, IServiceProvider serviceProvider)
        {
            _optionsMonitor = optionsMonitor;
            this.serviceProvider = serviceProvider;
        }

        public HttpClient CreateClient(string name)
        {
            var moniOptions = _optionsMonitor.Get(name);

            var mockHttp = new MockHttpMessageHandler();

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("http://localhost:5001/home/form").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post)
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/form").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post)
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("http://localhost:5001/home/testHeaderCollection").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post).WithHeaders(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("a", "a"), new KeyValuePair<string, string>("b", "b") })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/testInterceptor").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post).WithHeaders(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("d", "d") })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON


            mockHttp.When("http://localhost:5001/home/json")
                .WithContent("{\"Name\":\"sb\",\"Age\":3}")
                .With(it => it.Method == HttpMethod.Post)
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/multipart")
                .With(it =>
               {
                   if (it.Method == HttpMethod.Post && it.Content is MultipartFormDataContent formDataContent)
                   {
                       var parts = formDataContent.ToList();
                       var result0 = parts[0].ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                       var result1 = parts[1].ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                       var result2 = parts[2].ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                       Assert.Equal("sb", result0);
                       Assert.Equal("3", result1);
                       Assert.Equal("456", result2);
                       Assert.Equal("file", parts[2].Headers.ContentDisposition.Name);
                       return true;
                   }
                   else
                   {
                       return false;
                   }
               })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON


            mockHttp.When("http://localhost:5001/home/query").With(it =>
                {
                    if (it.Method == HttpMethod.Get&&it.RequestUri.Query== "?Name=sb&Age=3")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/queryWithExistCondition").With(it =>
                {
                    if (it.Method == HttpMethod.Get && it.RequestUri.Query == "?age=3&name=sb")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/QueryWithEscapeData").With(it =>
                {
                    if (it.Method == HttpMethod.Get && it.RequestUri.Query == "?Name=%E5%93%88%E5%93%88%E5%93%88&Age=3")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("application/json", "{\"Name\": \"哈哈哈\",\"Age\": 3}"); // Respond with JSON

            

            // Inject the handler or client into your application code
            var client = mockHttp.ToHttpClient();


            foreach (var action in moniOptions.HttpClientActions)
            {
                action(client);
            }

            return client;
        }
    }

    public class WrapDelegate : DelegatingHandler
    {

    }

    public class xxx : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return (builder) =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);
            };
        }
    }
}