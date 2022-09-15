using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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

            mockHttp.When("http://localhost:5001/home/testBasicAuthorization")
             .With(it => it.Method == HttpMethod.Get).WithHeaders(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Basic YWJjOjEyMw==") })
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
                    if (it.Method == HttpMethod.Get && it.RequestUri.Query == "?Name=sb&Age=3")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/downLoadWithStream").With(it =>
                {
                    if (it.Method == HttpMethod.Get)
                    {
                        return true;
                    }

                    return false;
                })
                .Respond(() =>
                {
                    var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(new FileInfo(basePath).OpenRead()),
                    };

                    response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        DispositionType = "attachment",
                        FileName = "123.txt",
                        FileNameStar = "123.txt",
                        Parameters = { }
                    };

                    //response.Content.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "123.txt"));
                    //response.Content.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "UTF-8''123.txt"));

                    return Task.FromResult<HttpResponseMessage>(response);
                }); // Respond with JSON



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

            mockHttp.When("http://localhost:5001/home/testHeadersWithInterfaceAndMethod").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post).WithHeaders(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("a", "a"), new KeyValuePair<string, string>("b", "b"), new KeyValuePair<string, string>("c", "c") })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/testHeadersWithInterface").WithFormData(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Name","sb"),
                    new KeyValuePair<string, string>("Age","3"),
                }).With(it => it.Method == HttpMethod.Post).WithHeaders(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("a", "a"), new KeyValuePair<string, string>("b", "b") })
                .Respond("application/json", "{\"Name\": \"sb\",\"Age\": 3}"); // Respond with JSON

            mockHttp.When("http://localhost:5001/home/testEmbedded").With(it =>
                {
                    //?Name=sb&Test={"Age":"3"}
                    if (it.Method == HttpMethod.Get && it.RequestUri.Query == "?Name=sb&Test=%7B%22Age%22%3A%223%22%7D")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("text/plain", "ok");

            mockHttp.When("http://localhost:5001/home/testNotEmbedded").With(it =>
                {
                    if (it.Method == HttpMethod.Get && it.RequestUri.Query == "?Name=sb&Age=3")
                    {
                        return true;
                    }

                    return false;
                })
                .Respond("text/plain", "ok");

            //添加cookie测试
            mockHttp.When("http://localhost:5001/home/TestCookieContainer1").With(it =>
                {
                    if (it.Method == HttpMethod.Get)
                    {
                        return true;
                    }

                    return false;
                })
                .Respond(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Set-Cookie", "abc=1;Path=/;Domain=localhost") }, new StringContent("ok"));
            //
            mockHttp.When("http://localhost:5001/home/TestCookieContainer2").With(it =>
                {
                    if (it.Method == HttpMethod.Get && it.Headers.TryGetValues("Cookie", out var values) && values.ToList().Contains("abc=1"))
                    {
                        return true;
                    }

                    return false;
                })
                .Respond(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Set-Cookie", "def=2") }, new StringContent("ok"));

            mockHttp.When("http://localhost:5001/home2/TestCookieContainer3").With(it =>
                {
                    if (it.Method == HttpMethod.Get)
                    {
                        return true;
                    }

                    return false;
                })
                .Respond(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Set-Cookie", "abc=1;Path=/home2;Domain=localhost") }, new StringContent("ok"));
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