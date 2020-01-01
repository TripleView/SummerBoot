using System.Reflection;

namespace SummerBoot.Cache
{
    public interface IKeyGenerator
    {
        string Generate(object target,MethodInfo method,params object[] param );
    }
}