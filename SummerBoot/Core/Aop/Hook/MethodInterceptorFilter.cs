using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace SummerBoot.Core.Aop.Hook
{
    public class MethodInterceptorFilter : IProxyGenerationHook//决定整个方法是否运用拦截器
    {

        private Func<Type, MethodInfo, bool> _func;


        public MethodInterceptorFilter(Func<Type, MethodInfo, bool> func)
        {
            _func = func;
        }

        /// <summary>
        /// proxy generator 生成Proxy中调用的最后一个方法。我们可以在这个方法中释放代码中占用的资源，比如数据库链接等
        /// </summary>
        public void MethodsInspected()
        {
            return;
        }

        /// <summary>
        /// 当proxy generator在生成Proxy中，遇到没有virtual的方法的时候会调用这个方法，询问用户应该如何处理
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberInfo"></param>
        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
            return;
        }


        /// <summary>
        /// proxy generator在生成Proxy的过程中，询问用户此方法是否应该被拦截。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return _func(type, methodInfo);
        }
    }
}