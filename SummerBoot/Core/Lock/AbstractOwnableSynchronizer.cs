using System;
using System.Threading;

namespace SummerBoot.Core.Lock
{
    [Serializable]
    public abstract class AbstractOwnableSynchronizer
    {
        protected Thread ExclusiveOwnerThread { set; get; }

        protected AbstractOwnableSynchronizer() { }

       
    }



}