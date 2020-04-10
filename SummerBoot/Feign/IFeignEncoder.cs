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
        /// <summary>
        /// 序列化post中的body数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="requestTemplate"></param>
        void Encoder(object obj, RequestTemplate requestTemplate);

        void EncoderFormValue(object obj, RequestTemplate requestTemplate);
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

            public void EncoderFormValue(object obj, RequestTemplate requestTemplate)
            {
                var objStr = JsonConvert.SerializeObject(obj);
                var dic =JsonConvert.DeserializeObject<Dictionary<string, string>>(objStr);
                requestTemplate.FormValue =dic.ToList();
            }
        }
    }
}
