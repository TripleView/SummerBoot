using System.Reflection;

namespace SummerBoot.Cache
{
    public interface ICacheOperationContext<T> where T:CacheOperation
    {
        T GetOperation();

        object GetTarget();

        MethodInfo GetMethod();

        object[] GetArgs();
    }
}