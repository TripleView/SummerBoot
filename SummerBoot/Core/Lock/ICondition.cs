using System;

namespace SummerBoot.Core.Lock
{
    public interface ICondition
    {
        void Await();
        void AwaitUninterruptibly();
        long AwaitNanos(long nanos);
        bool Await(long timeOut, TimeSpan timeSpan);
        bool AwaitUntil(DateTime deadline);
        void Signal();

        void SignalAll();
    }
}