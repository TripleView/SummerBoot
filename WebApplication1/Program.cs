using SummerBoot.Core;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1
{
    /// <summary>
    /// �ʼ�������
    /// </summary>
    public class SendBody
    {
        /// <summary>
        /// ģ����
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// ���͵�����
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// ���ͷ�ʽ,1�ʼ���2΢��
        /// </summary>
        public List<int> SendTypes { get; set; }
        /// <summary>
        /// �����б�
        /// </summary>
        public List<SendBodyVariable> Variables { get; set; }
    }

    /// <summary>
    /// ����
    /// </summary>
    public class SendBodyVariable
    {
        /// <summary>
        /// ��������
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ����ֵ
        /// </summary>
        public string Value { get; set; }
    }
    /// <summary>
    /// �����ļ�dto
    /// </summary>
    public class SendBodyFile
    {
        /// <summary>
        /// �ļ���
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// �ļ�����
        /// </summary>
        public byte[] FileBytes { get; set; }
    }
    /// <summary>
    /// ���ͽ��
    /// </summary>
    public class SendNoticeResult
    {
        public string Code { get; set; }
        public string Msg { get; set; }

        public string Data { get; set; }
    }

    /// <summary>
    /// ֪ͨ����
    /// </summary>
    public class NoticeCenterService
    {
        /// <summary>
        /// ����֪ͨ
        /// </summary>
        /// <param name="code">ģ��code</param>
        /// <param name="language">����</param>
        /// <param name="sendTypes">��������</param>
        /// <param name="variables">����</param>
        /// <param name="attachments">����</param>
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
            var file1Bytes = File.ReadAllBytes("E:\\�½��ļ���3\\Dockerfile");
            var file2Bytes = File.ReadAllBytes("E:\\�½��ļ���4\\123.txt");

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
                    Name = "����1",
                    Value = "���Ǳ���1��ֵ��"
                }
            };

            new NoticeCenterService().SendNoticeAsync("Em00000007", "2052", new List<int>() { 1 }, variables, files).GetAwaiter().GetResult();
        }
    }
}