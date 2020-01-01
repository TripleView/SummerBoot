using System;

namespace SummerBoot.Core.Lock
{
    public interface ILock
    {
        void Lock();
        void LockInterruptibly();
        bool TryLock();

        bool TryLock(long timeOut, TimeSpan timeSpan);

        void Unlock();
        ICondition NewCondition();
    }
}