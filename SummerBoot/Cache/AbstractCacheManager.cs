using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;

namespace SummerBoot.Cache
{
    public abstract class AbstractCacheManager : ICacheManager
    {
        private readonly ConcurrentDictionary<string, ICache> _cacheMap = new ConcurrentDictionary<string, ICache>(16, 16);
        private volatile IList<string> _cacheNames = new List<string>();
        private static readonly object LockObj = new object();
        /// <summary>
        /// 初始化缓存
        /// </summary>
        public void InitializeCaches()
        {
            //模板模式
            //首先加载缓存。loadCaches是一个模板方法，具体怎么加载子类决定。
            IList<ICache> caches = LoadCaches();

            lock (LockObj)
            {
                //清空缓存CacheMap  ConcurrentDictionary
                this._cacheMap.Clear();
                //每次初始化都创建一个新的缓存名称Set。代码最后一行，把这个set集合变成只读
                this._cacheNames = new List<string>();
                //1. 循环遍历子类加载的caches
                ISet<string> tmpCacheNames = new HashSet<string>(caches.Count);
                foreach (ICache cache in caches)
                {
                    string name = cache.Name;
                    //加入到缓存map集合中，在加入前要进行对缓存进行装配 decorateCache(cache)
                    //decorateCache 这个方法本身就是返回cache。但是子类
                    //AbstractTransactionSupportingCacheManager 重写它。这个类从名字可以看出
                    //是支持事务。它有一个属性transactionAware默认为false. 如果配置了支持事务，
                    //就会把当前cache装配成支持事物的cache 所以后面会有支持事务的配置，配置的就是
                    //transactionAware 这个属性为true
                    //TransactionAwareCacheDecorator
                    this._cacheMap.TryAdd(name, DecorateCache(cache));
                    //2. 把cache的name加入到name集合中
                    _cacheNames.Add(name);
                }

                this._cacheNames = this._cacheNames.ToImmutableList();
            }

        }

        protected abstract IList<ICache> LoadCaches();

        public abstract string GetName();

        protected virtual ICache DecorateCache(ICache cache)
        {
            return cache;
        }

        public ICache GetCache(string name)
        {
            this._cacheMap.TryGetValue(name, out var cache);
            if (cache != null)
            {
                return cache;
            }
            else
            {
                lock (LockObj)
                {
                    cache = GetMissingCache(name);
                    if (cache != null)
                    {
                        this.AddCache(cache);
                    }
                }
                return cache;
            }
        }

        public IList<string> GetCacheNames()
        {
            return this._cacheNames;
        }

        protected virtual ICache GetMissingCache(string name)
        {
            return null;
        }

        protected void AddCache(ICache cache)
        {
            string name = cache.Name;
            var addSuccess = this._cacheMap.TryAdd(name, DecorateCache(cache));

            if (addSuccess) UpdateCacheNames(name);
        }

        private void UpdateCacheNames(string name)
        {
            IList<string> tmpCacheNames = new List<string>(this._cacheNames.Count + 1);
            foreach (var cacheName in this._cacheNames)
            {
                tmpCacheNames.Add(cacheName);
            }

            tmpCacheNames.Add(name);
            this._cacheNames = tmpCacheNames.ToImmutableList();
        }

        protected ICache LookUpCache(string name)
        {
            this._cacheMap.TryGetValue(name, out var cache);
            return cache;
        }

        public virtual void AfterPropertiesSet()
        {
            this.InitializeCaches();
        }
    }
}