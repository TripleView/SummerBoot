namespace SummerBoot.Cache
{
    public class SbCacheOption
    {
        /// <summary>
        /// 是否开启链式调用
        /// </summary>
        public bool ChainedCall { set; get; }

        /// <summary>
        /// 是否启用redis缓存
        /// </summary>
        public bool IsUseRedis { get; private set; } = false;

        public string RedisConnectionStr {private set; get; }
        /// <summary>
        /// 是否启用内存缓存
        /// </summary>
        public bool IsUseMemory { get; private set; } = false;

        /// <summary>
        /// 启用redis缓存
        /// </summary>
        public void UseRedis(string connectionStr)
        {
            this.IsUseRedis = true;
            this.RedisConnectionStr = connectionStr;
        }

        /// <summary>
        /// 启用内存缓存
        /// </summary>
        public void UseMemory()
        {
            this.IsUseMemory = true;
           
        }
    }
}