using System;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
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
            mockHttp.When("http://localhost/api/user/*")
                .Respond("application/json", "{'name' : 'Test McGee'}"); // Respond with JSON

        
            // Inject the handler or client into your application code
            var client = mockHttp.ToHttpClient();
            

            foreach (var action in moniOptions.HttpClientActions)
            {
                action(client);
            }

            var dd = new xxx();
            var f = (HttpMessageHandlerBuilder)serviceProvider.GetService(typeof(HttpMessageHandlerBuilder));
            Action<HttpMessageHandlerBuilder> fff;
            foreach (var action in moniOptions.HttpMessageHandlerBuilderActions)
            {
                action(f);
            }

            
            var handler = f.Build();
            var a1= new WrapDelegate()
            {
                InnerHandler = handler
            };

            var a2 = new WrapDelegate()
            {
                InnerHandler = a1
            };


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