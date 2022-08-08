using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using SummerBoot.Core;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign
{
    /// <summary>
    /// feign工作单元
    /// </summary>
    public interface IFeignUnitOfWork
    {
        /// <summary>
        /// 引用次数，开启一次事物加+1,当次数为1时提交，主要是为了防止事物嵌套
        /// </summary>
        int ActiveNumber { get; set; }

        bool IsShareCookie => ActiveNumber > 0;
        /// <summary>
        /// 开启cookie共享
        /// </summary>
        void BeginCookie();
        /// <summary>
        /// 结束cookie共享
        /// </summary>
        void StopCookie();

        bool AddCookie(string url, string cookieString);
        bool AddCookie(string url, Cookie cookie);
        List<Cookie> GetCookies(string url);
    }

    public class DefaultFeignUnitOfWork : IFeignUnitOfWork
    {
        private CookieContainer cookieContainer = new CookieContainer();
        public int ActiveNumber { get; set; } = 0;

        public bool AddCookie(string url, string cookieString)
        {
            if (ActiveNumber == 0)
            {
                throw new Exception("Unopened beginCookie");
            }
            cookieContainer.SetCookies(new Uri(url), cookieString);
            return true;
        }
        public bool AddCookie(string url, Cookie cookie)
        {
            if (ActiveNumber == 0)
            {
                throw new Exception("Unopened beginCookie");
            }

            cookieContainer.Add(new Uri(url), cookie);
            return true;
        }

        public void BeginCookie()
        {
            ActiveNumber++;
        }

        public List<Cookie> GetCookies(string url)
        {
            if (ActiveNumber == 0)
            {
                throw new Exception("Unopened beginCookie");
            }
            var urlCookies = cookieContainer.GetCookies(new Uri(url));
            var result = urlCookies.ToList();
            return result;
        }

        public void StopCookie()
        {
            ActiveNumber--;
        }
    }

}