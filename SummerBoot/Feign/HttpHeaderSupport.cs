using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace SummerBoot.Feign
{
    public static class HttpHeaderSupport
    {
        public static readonly IList<string> RequestHeaders=new List<string>();
        public static readonly IList<string> ContentHeaders = new List<string>();

        static HttpHeaderSupport()
        {
             RequestHeaders = typeof(HttpRequestHeaders).GetProperties().Select(it=>it.Name.ToUpper()).ToList();
             ContentHeaders = typeof(HttpContentHeaders).GetProperties().Select(it => it.Name.ToUpper()).ToList();
        }

        public static void Init()
        {

        }
    }
}