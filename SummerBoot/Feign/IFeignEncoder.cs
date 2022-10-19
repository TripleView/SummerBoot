using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SummerBoot.Feign
{
    /// <summary>
    /// 序列化接口
    /// </summary>
    public interface IFeignEncoder
    {
        /// <summary>
        /// 序列化post中的body数据
        /// </summary>
        /// <param name="obj"></param>
        HttpContent Encoder(object obj);
        /// <summary>
        /// 序列化obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string EncoderObject(object obj);

        /// <summary>
        /// 默认的序列化器
        /// </summary>
        public class DefaultEncoder : IFeignEncoder
        {
            private JsonSerializerSettings settings = new JsonSerializerSettings()
                { ContractResolver = new FeignContractResolver() };
            public HttpContent Encoder(object obj)
            {
                var c = JsonConvert.SerializeObject(obj, settings);
                var content = new StringContent(JsonConvert.SerializeObject(obj, settings), Encoding.UTF8, "application/json");

                return content;
            }

            public string EncoderObject(object obj)
            {
                return JsonConvert.SerializeObject(obj, settings );
            }
        }
    }
}
