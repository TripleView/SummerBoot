using System;
using System.Text;

namespace SummerBoot.Feign
{

    public class BasicAuthorization
    {
        public BasicAuthorization(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string GetBaseAuthString()
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"));
        }
    }
}