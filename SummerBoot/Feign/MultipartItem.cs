using System.IO;
using System.Net.Http;

namespace SummerBoot.Feign
{
   /// <summary>
   /// 文件上传类
   /// </summary>
    public class MultipartItem
    {
        
        public string Name { get;  }
        public string FileName { get;}

        public HttpContent Content { get; }

        public MultipartItem(Stream stream, string name, string fileName)
        {
            this.Content = new StreamContent(stream);
            this.Name = name;
            this.FileName = fileName;
        }

        public MultipartItem(FileInfo fileInfo, string name, string fileName)
        {
            this.Content = new StreamContent(fileInfo.OpenRead());
            this.Name = name;
            this.FileName = fileName;
        }

        public MultipartItem(byte[] bytes, string name, string fileName)
        {
            this.Content =new ByteArrayContent(bytes);
            this.Name = name;
            this.FileName = fileName;
        }
    }
}