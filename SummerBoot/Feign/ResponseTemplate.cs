using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public class ResponseTemplate
    {
        public HttpStatusCode HttpStatusCode { set; get; }

        public Stream Body { set; get; }

        public IDictionary<string, IEnumerable<string>> Headers { get; set; } = new Dictionary<string, IEnumerable<string>>();

        public HttpResponseMessage OrignHttpResponseMessage { set; get; }

    }
}
