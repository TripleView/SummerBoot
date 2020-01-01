using System;
using System.Threading;

namespace SummerBoot.Core
{
    public class SynchronizedAttribute
    {
        public void test()
        {
           SpinLock s=new SpinLock();
            //SynchronizationContext
        }
    }
}