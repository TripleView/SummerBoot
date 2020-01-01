using System;

namespace SummerBoot.Core.Lock
{
    [Serializable]
    public class ReentrantLock : ILock
    {
        public void Lock()
        {
      
            throw new NotImplementedException();
        }

        public void LockInterruptibly()
        {
            throw new NotImplementedException();
        }

        public ICondition NewCondition()
        {
            throw new NotImplementedException();
        }

        public bool TryLock()
        {
            throw new NotImplementedException();
        }

        public bool TryLock(long timeOut, TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        public void Unlock()
        {
            throw new NotImplementedException();
        }
    }
}