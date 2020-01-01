using System.Collections.Generic;
using System.Linq;

namespace SummerBoot.Cache
{
    public interface ICacheResolver
    {
        ICollection<ICache> CacheResolve(ICacheOperationContext<CacheOperation> context);
    }
}