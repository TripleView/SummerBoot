using System;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public abstract class AbstractTransactionSupportingCacheManager : AbstractCacheManager
    {
        private bool _transactionAware = false;

        private IUnitOfWork Uow { set; get; }

        protected AbstractTransactionSupportingCacheManager(IUnitOfWork uow)
        {
            this.Uow = uow;
        }
        /**
         * Set whether this CacheManager should expose transaction-aware Cache objects.
         * <p>Default is "false". Set this to "true" to synchronize cache put/evict
         * operations with ongoing Spring-managed transactions, performing the actual cache
         * put/evict operation only in the after-commit phase of a successful transaction.
         */
        public void SetTransactionAware(bool transactionAware)
        {
            this._transactionAware = transactionAware;
        }

        /**
         * Return whether this CacheManager has been configured to be transaction-aware.
         */
        public bool IsTransactionAware()
        {
            return this._transactionAware;
        }

        protected override ICache DecorateCache(ICache cache)
        {
            return IsTransactionAware() ? new TransactionAwareCacheDecorator(cache, Uow) : cache;
        }
    }
}