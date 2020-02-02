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
    public interface IDecoder
    {
        object Decoder(ResponseTemplate responseTemplate, Type type);

        T Decoder<T>(ResponseTemplate responseTemplate);
        public class DefaultDecoder : IDecoder
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
                return JsonConvert.DeserializeObject(str, type);
            }

            public T Decoder<T>(ResponseTemplate responseTemplate)
            {
                return (T)this.Decoder(responseTemplate, typeof(T));
            }
        }
    }
}
