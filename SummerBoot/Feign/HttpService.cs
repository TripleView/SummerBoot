using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using  Newtonsoft.Json;

namespace demo.Service
{
    public class HttpService
    {
        public string Name { set; get; }
        //public async Task<T> GetAsync<T>() where T:class
        //{
        //    var url = "http://localhost:5000/home/test";
        //    HttpClient t=new HttpClient();
        //    var f=await t.GetAsync(url);
        //    return JsonConvert.DeserializeObject<T>(await f.Content.ReadAsStringAsync());
        //}

        public async Task<User> GetAsync()        {
            var url = "http://localhost:5000/home/test";
            HttpClient t = new HttpClient();
            var f = await t.GetAsync(url);
            return JsonConvert.DeserializeObject<User>(await f.Content.ReadAsStringAsync());
        }
    }
}