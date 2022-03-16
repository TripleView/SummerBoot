using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using SummerBoot.Feign;

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
            var moniOptions= _optionsMonitor.Get(name);

            var mockHttp = new MockHttpMessageHandler();

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("http://localhost:5001/home/form").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it=>it.Method==HttpMethod.Post)
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
                }).With(it => it.Method == HttpMethod.Post).WithHeaders(new List<KeyValuePair<string, string>>(){new KeyValuePair<string, string>("a","a"), new KeyValuePair<string, string>("b", "b") })
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

            mockHttp.When("http://localhost:5001/home/uploadPart")
                .WithContent("{\"Name\":\"sb\",\"Age\":3}")
                
                .With(it => it.Method == HttpMethod.Post)
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            // Inject the handler or client into your application code
            var client = mockHttp.ToHttpClient();
            

            foreach (var action in moniOptions.HttpClientActions)
            {
                action(client);
            }

            return client;
        }
    }

    public class WrapDelegate:DelegatingHandler
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