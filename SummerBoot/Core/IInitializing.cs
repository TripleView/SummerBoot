namespace SummerBoot.Core
{
    /// <summary>
    /// 在生成动态代理类,并执行属性注入后,执行方法；
    /// </summary>
    public interface IInitializing
    {
        void AfterPropertiesSet();
    }
}