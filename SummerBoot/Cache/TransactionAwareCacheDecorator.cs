

using System;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class TransactionAwareCacheDecorator : ICache
    {
        private IUnitOfWork Uow { set; get; }

        public string Name
        {
            get => this._targetCache.Name;
            set { }
        }

        public object GetNativeCache
        {
            get => this._targetCache.GetNativeCache;
        }

        private readonly ICache _targetCache;

        public TransactionAwareCacheDecorator(ICache targetCache, IUnitOfWork uow)
        {
            this._targetCache = targetCache;
            this.Uow = uow;
        }

        public ICache GetTargetCache()
        {
            return this._targetCache;
        }

        public void Clear()
        {
            if (Uow.ActiveNumber > 0)
            {
                Uow.RegisterCallBack(() =>
                {
                    this._targetCache.Clear();
                });
            }
            else
            {
                this._targetCache.Clear();
            }

        }

        public void Evict(string key)
        {
            if (Uow.ActiveNumber > 0)
            {
                Uow.RegisterCallBack(() =>
                {
                    this._targetCache.Evict(key);
                });
            }
            else
            {
                this._targetCache.Evict(key);
            }

        }


        public void Put(string key, object value)
        {
            if (Uow.ActiveNumber > 0)
            {
                Uow.RegisterCallBack(() =>
                {
                    this._targetCache.Put(key, value);
                });
            }
            else
            {
                this._targetCache.Put(key, value);
            }
        }

        public T Get<T>(string key, Func<T> callBack)
        {
            return this._targetCache.Get<T>(key, callBack);
        }

        public IValueWrapper Get(string key, Type returnType, Func<object> callBack = null)
        {
            return this._targetCache.Get(key, returnType, callBack);
        }
    }
}