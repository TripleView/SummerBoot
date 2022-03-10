using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        /// 默认的序列化器
        /// </summary>
        public class DefaultEncoder : IFeignEncoder
        {
            public HttpContent Encoder(object obj)
            {
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                return content;
            }

        }
    }
}
