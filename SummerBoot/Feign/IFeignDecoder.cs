using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SummerBoot.Core;

namespace SummerBoot.Feign
{
    /// <summary>
    /// 反序列化接口
    /// </summary>
    public interface IFeignDecoder
    {
        object Decoder(ResponseTemplate responseTemplate, Type type);

        T Decoder<T>(ResponseTemplate responseTemplate);

        /// <summary>
        /// 默认的反序列化器
        /// </summary>
        public class DefaultDecoder : IFeignDecoder
        {
            public object Decoder(ResponseTemplate responseTemplate, Type type)
            {
                if (responseTemplate.HttpStatusCode == HttpStatusCode.NotFound ||
                    responseTemplate.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    return type.GetDefaultValue();
                }

                if (responseTemplate.Body == null) return null;
                
                var body = responseTemplate.Body;
                var str = body.ConvertToString();
                if (responseTemplate.HttpStatusCode != HttpStatusCode.OK) throw new Exception(str);
                
                //var json = new JsonSerializer().Deserialize(new JsonTextReader(new StringReader(str)), type)
                return JsonConvert.DeserializeObject(str, type);
            }

            public T Decoder<T>(ResponseTemplate responseTemplate)
            {
                return (T)this.Decoder(responseTemplate, typeof(T));
            }
        }
    }
}
