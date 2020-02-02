using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SummerBoot.Feign
{
    public interface IEncoder
    {
        void Encoder(object obj, RequestTemplate requestTemplate);
        public class DefaultEncoder : IEncoder
        {
            public void Encoder(object obj, RequestTemplate requestTemplate)
            {
                var objStr = JsonConvert.SerializeObject(obj);
                requestTemplate.Body = objStr;
            }
        }
    }
}
