using System.Net.Http;

namespace SummerBoot.Feign
{
    public class FeignHttpClientHandler: HttpClientHandler
    {
        public FeignHttpClientHandler()
        {
            UseCookies = false;
        }
    }
}

