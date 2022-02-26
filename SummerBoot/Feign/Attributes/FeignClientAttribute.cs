using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FeignClientAttribute:Attribute
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// url地址
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// 回退类
        /// </summary>
        public Type FallBack { get; }
        public Type Configuration { get; }
        public bool Decode404 { get; }
        public string Qualifier { get; }
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <param name="url">url地址</param>
        /// <param name="fallBack">回退类</param>
        /// <param name="configuration"></param>
        /// <param name="decode404"></param>
        /// <param name="qualifier"></param>
        /// <param name="path">路径</param>
        public FeignClientAttribute(string name,string url="",Type fallBack=null,Type configuration=null,bool decode404=false,string qualifier="",string path="")
        {
            Name = name;
            Url = url;
            FallBack = fallBack;
            Configuration = configuration;
            Decode404 = decode404;
            Qualifier = qualifier;
            Path = path;
        }
    }
}
