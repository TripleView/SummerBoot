using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SummerBoot.Core.Lock
{
    public static class LockSupport
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0);

        private static readonly ConcurrentDictionary<int,SemaphoreSlim> dic=new ConcurrentDictionary<int, SemaphoreSlim>();
    

        public static void UnPark()
        {
            
            var id = Thread.CurrentThread.ManagedThreadId;
            var b_exist = dic.TryGetValue(id, out var lockobj);
            if(b_exist)lockobj.Release();
        }

        public static void UnPark(Thread t)
        {

            var id = t.ManagedThreadId;
            var b_exist = dic.TryGetValue(id, out var lockobj);
            if (b_exist) lockobj.Release();
        }
        public static void Park()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var b_exist = dic.TryGetValue(id, out var lockobj);
            if (b_exist)
            {
                lockobj.Wait();
                return;
            }

            lockobj=new SemaphoreSlim(0);
            dic.TryAdd(id, lockobj);
            lockobj.Wait();
        }

        public static void ParkNanos(long nanos)
        {
            if (nanos > 0L)
            {
                var time = (int) nanos / 1000;
     

                var id = Thread.CurrentThread.ManagedThreadId;
                var b_exist = dic.TryGetValue(id, out var lockobj);
                if (b_exist)
                {
                    lockobj.Wait(time);
                    return;
                }

                lockobj = new SemaphoreSlim(0);
                dic.TryAdd(id, lockobj);
                lockobj.Wait(time);
            }

        }

        public static void ParkUntil(long deadline)
        {
            if (deadline > 0L)
            {
                var time = (int)(deadline-SbUtil.CurrentTimeMillis()) / 1000;


                var id = Thread.CurrentThread.ManagedThreadId;
                var b_exist = dic.TryGetValue(id, out var lockobj);
                if (b_exist)
                {
                    lockobj.Wait(time);
                    return;
                }

                lockobj = new SemaphoreSlim(0);
                dic.TryAdd(id, lockobj);
                lockobj.Wait(time);
            }
        }
    }
} 