using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SummerBoot.Cache
{
    public class SimpleKeyGenerator : IKeyGenerator
    {
        private char separator = ':';
        public string Generate(object target, MethodInfo method, params object[] param)
        {
            var result = $"{method.DeclaringType?.Name}{separator}{method.Name}{separator}{GenerateKey(param)}";
            return result;
        }

        private string GenerateKey(params object[] param)
        {
            if (param == null|| param.Length == 0) return "0";
            return string.Join(separator, param);
        }
    }
}