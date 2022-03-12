using System.IO;
using System.Net.Http;

namespace SummerBoot.Feign
{
   /// <summary>
   /// 文件上传类
   /// </summary>
    public class MultipartItem
    {
        
        public string Name { get; set; }
        public string FileName { get; set; }

        public HttpContent Content { get; }

        public MultipartItem(Stream stream)
        {
            this.Content = new StreamContent(stream);
        }

        public MultipartItem(FileInfo fileInfo)
        {
            this.Content = new StreamContent(fileInfo.OpenRead());
        }

        public MultipartItem(byte[] bytes)
        {
            this.Content =  new ByteArrayContent(bytes);
        }

        public MultipartItem(string str)
        {
            this.Content = new StringContent(str);
        }
    }
}