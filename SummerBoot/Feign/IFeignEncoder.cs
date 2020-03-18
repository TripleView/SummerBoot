using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SummerBoot.Feign
{
    /// <summary>
    /// 序列化接口
    /// </summary>
    public interface IFeignEncoder
    {
        void Encoder(object obj, RequestTemplate requestTemplate);
        /// <summary>
        /// 默认的序列化器
        /// </summary>
        public class DefaultEncoder : IFeignEncoder
        {
            public void Encoder(object obj, RequestTemplate requestTemplate)
            {
                var objStr = JsonConvert.SerializeObject(obj);
                requestTemplate.Body = objStr;
            }
        }
    }
}
