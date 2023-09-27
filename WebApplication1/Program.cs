using SummerBoot.Core;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1
{
    /// <summary>
    /// 邮件请求体
    /// </summary>
    public class SendBody
    {
        /// <summary>
        /// 模板编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 发送的语言
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 发送方式,1邮件，2微信
        /// </summary>
        public List<int> SendTypes { get; set; }
        /// <summary>
        /// 变量列表
        /// </summary>
        public List<SendBodyVariable> Variables { get; set; }
    }

    /// <summary>
    /// 变量
    /// </summary>
    public class SendBodyVariable
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public string Value { get; set; }
    }
    /// <summary>
    /// 发送文件dto
    /// </summary>
    public class SendBodyFile
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件内容
        /// </summary>
        public byte[] FileBytes { get; set; }
    }
    /// <summary>
    /// 发送结果
    /// </summary>
    public class SendNoticeResult
    {
        public string Code { get; set; }
        public string Msg { get; set; }

        public string Data { get; set; }
    }

    /// <summary>
    /// 通知服务
    /// </summary>
    public class NoticeCenterService
    {
        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="code">模板code</param>
        /// <param name="language">语言</param>
        /// <param name="sendTypes">发送类型</param>
        /// <param name="variables">变量</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        public async Task<bool> SendNoticeAsync(string code, string language, List<int> sendTypes, List<SendBodyVariable> variables, List<SendBodyFile> attachments)
        {
            var requestContent = new MultipartFormDataContent();
            var body = new SendBody()
            {
                Code = code,
                Language = language,
                SendTypes = sendTypes,
                Variables = variables
            };

            var requestBody = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(requestBody);
            requestContent.Add(stringContent, "Parameters");

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    var fileContent = new ByteArrayContent(attachment.FileBytes);
                    requestContent.Add(fileContent, "File", attachment.FileName);
                }
            }

            using (var client = new HttpClient())
            {
                using (var r = await client.PostAsync("http://emailcenterapi.atlbattery.com/api/Send/SendWithAttachFile", requestContent))
                {
                    string result = await r.Content.ReadAsStringAsync();
                    var sendResult = JsonConvert.DeserializeObject<SendNoticeResult>(result);
                    if (sendResult?.Code == "20000")
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            var file1Bytes = File.ReadAllBytes("E:\\新建文件夹3\\Dockerfile");
            var file2Bytes = File.ReadAllBytes("E:\\新建文件夹4\\123.txt");

            var files = new List<SendBodyFile>()
            {
                new SendBodyFile()
                {
                    FileBytes = file1Bytes,
                    FileName = "Dockerfile"
                },
                new SendBodyFile()
                {
                    FileBytes = file2Bytes,
                    FileName = "123.txt"
                },
            };

            var variables = new List<SendBodyVariable>()
            {
                new SendBodyVariable()
                {
                    Name = "变量1",
                    Value = "我是变量1的值啊"
                }
            };

            new NoticeCenterService().SendNoticeAsync("Em00000007", "2052", new List<int>() { 1 }, variables, files).GetAwaiter().GetResult();
        }
    }
}