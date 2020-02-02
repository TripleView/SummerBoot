using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FeignClientAttribute:Attribute
    {
        public string Name { get; }
        public string Url { get; }
        public Type FallBack { get; }
        public Type Configuration { get; }
        public bool Decode404 { get; }
        public string Qualifier { get; }
        public string Path { get; }
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
