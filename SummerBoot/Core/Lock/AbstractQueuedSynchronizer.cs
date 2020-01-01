//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using SummerBoot.Core.Lock;

//namespace SummerBoot.Core.Lock
//{
//    public abstract class AbstractQueuedSynchronizer : AbstractOwnableSynchronizer
//    {
//        protected AbstractQueuedSynchronizer()
//        {
//            var b=new SemaphoreSlim(0);
//            var c=new SpinLock();
//        }

//        private volatile Node head;
//        private volatile Node tail;
//        private volatile int state;
//        const long SPIN_FOR_TIMEOUT_THRESHOLD = 1000L;

//        protected int GetState()
//        {
//            return this.state;
//        }

//        protected void SetState(int newState)
//        {
//            this.state = newState;
//        }

//        protected bool CompareAndSetState(int expect, int update)
//        {
//            return Interlocked.CompareExchange(ref state, update, expect) == expect;
//        }

//        private Node Enq(Node node)
//        {
//            while (true)
//            {
//                var oldTail = this.tail;
//                if (oldTail != null)
//                {
//                    node.SetPrevRelaxed(oldTail);
//                    if (this.CompareAndSetTail(oldTail, node))
//                    {
//                        oldTail.next = node;
//                        return oldTail;
//                    }
//                }
//                else
//                {
//                    this.InitializeSyncQueue();
//                }
//            }
//        }

//        private Node AddWaiter(Node mode)
//        {
//            var node = new Node(mode);

//            Node oldTail;
//            do
//            {
//                while (true)
//                {
//                    oldTail = this.tail;
//                    if (oldTail != null)
//                    {
//                        node.SetPrevRelaxed(oldTail);
//                        break;
//                    }

//                    this.InitializeSyncQueue();
//                }
//            } while (!this.CompareAndSetTail(oldTail, node));

//            oldTail.next = node;
//            return node;
//        }

//        private void SetHead(Node node)
//        {
//            this.head = node;
//            node.thread = null;
//            node.prev = null;
//        }

//        private void UnParkSuccessor(Node node)
//        {
//            int ws = node.waitStatus;
//            if (ws < 0)
//            {
//                node.CompareAndSetWaitStatus(ws, 0);
//            }

//            var s = node.next;
//            if (s == null || s.waitStatus > 0)
//            {
//                s = null;

//                for (var p = this.tail; p != node && p != null; p = p.prev)
//                {
//                    if (p.waitStatus <= 0)
//                    {
//                        s = p;
//                    }
//                }
//            }

//            if (s != null)
//            {

//                LockSupport.UnPark(s.thread);
//            }

//        }

//        private void DoReleaseShared()
//        {
//            while (true)
//            {
//                var h = this.head;
//                if (h != null && h != this.tail)
//                {
//                    int ws = h.waitStatus;
//                    if (ws == -1)
//                    {
//                        if (!h.CompareAndSetWaitStatus(-1, 0))
//                        {
//                            continue;
//                        }

//                        this.UnParkSuccessor(h);
//                    }
//                    else if (ws == 0 && !h.CompareAndSetWaitStatus(0, -3))
//                    {
//                        continue;
//                    }
//                }

//                if (h == this.head)
//                {
//                    return;
//                }
//            }
//        }

//        private void SetHeadAndPropagate(Node node, int propagate)
//        {
//            var h = this.head;
//            this.SetHead(node);
//            if (propagate > 0 || h == null || h.waitStatus < 0 || (h = this.head) == null || h.waitStatus < 0)
//            {
//                var s = node.next;
//                if (s == null || s.IsShared())
//                {
//                    this.DoReleaseShared();
//                }
//            }

//        }

//        private void CancelAcquire(Node node)
//        {
//            if (node != null)
//            {
//                node.thread = null;

//                Node pred;
//                for (pred = node.prev; pred.waitStatus > 0; node.prev = pred = pred.prev)
//                {
//                }

//                var predNext = pred.next;
//                node.waitStatus = 1;
//                if (node == this.tail && this.CompareAndSetTail(node, pred))
//                {
//                    pred.CompareAndSetNext(predNext, null);
//                }
//                else
//                {
//                    int ws;
//                    if (pred != this.head && ((ws = pred.waitStatus) == -1 || ws <= 0 && pred.CompareAndSetWaitStatus(ws, -1)) && pred.thread != null)
//                    {
//                        var next = node.next;
//                        if (next != null && next.waitStatus <= 0)
//                        {
//                            pred.CompareAndSetNext(predNext, next);
//                        }
//                    }
//                    else
//                    {
//                        this.UnParkSuccessor(node);
//                    }

//                    node.next = node;
//                }

//            }
//        }

//        private static bool ShouldParkAfterFailedAcquire(Node pred, Node node)
//        {
//            int ws = pred.waitStatus;
//            if (ws == -1)
//            {
//                return true;
//            }
//            else
//            {
//                if (ws > 0)
//                {
//                    do
//                    {
//                        node.prev = pred = pred.prev;
//                    } while (pred.waitStatus > 0);

//                    pred.next = node;
//                }
//                else
//                {
//                    pred.CompareAndSetWaitStatus(ws, -1);
//                }

//                return false;
//            }
//        }

//        static void SelfInterrupt()
//        {
//            Thread.CurrentThread.Interrupt();
//        }

//        private bool ParkAndCheckInterrupt()
//        {
//            LockSupport.Park();
//            Thread.CurrentThread.Interrupt();
//            return true;

//        }

//        public bool AcquireQueued(Node node, int arg)
//        {
//            var interrupted = false;

//            try
//            {
//                while (true)
//                {
//                    var p = node.Predecessor();
//                    if (p == this.head && this.TryAcquire(arg))
//                    {
//                        this.SetHead(node);
//                        p.next = null;
//                        return interrupted;
//                    }

//                    if (ShouldParkAfterFailedAcquire(p, node))
//                    {
//                        interrupted |= this.ParkAndCheckInterrupt();
//                    }
//                }
//            }
//            catch (Exception var5)
//            {
//                this.CancelAcquire(node);
//                if (interrupted)
//                {
//                    SelfInterrupt();
//                }

//                throw var5;
//            }
//        }

//        private void DoAcquireInterruptibly(int arg)
//        {
//            var node = this.AddWaiter(AbstractQueuedSynchronizer.Node.EXCLUSIVE);

//            try
//            {
//                Node p;
//                do
//                {
//                    p = node.Predecessor();
//                    if (p == this.head && this.TryAcquire(arg))
//                    {
//                        this.SetHead(node);
//                        p.next = null;
//                        return;
//                    }
//                } while (!ShouldParkAfterFailedAcquire(p, node) || !this.ParkAndCheckInterrupt());

//                throw new ThreadInterruptedException();
//            }
//            catch (Exception var4)
//            {
//                this.CancelAcquire(node);
//                throw var4;
//            }
//        }

//        private bool DoAcquireNanos(int arg, long nanosTimeout)
//        {
//            if (nanosTimeout <= 0L)
//            {
//                return false;
//            }

//            long deadline = SbUtil.NanoTime() + nanosTimeout;
//            var node = this.AddWaiter(Node.EXCLUSIVE);

//            try
//            {
//                do
//                {
//                    var p = node.Predecessor();
//                    if (p == this.head && this.TryAcquire(arg))
//                    {
//                        this.SetHead(node);
//                        p.next = null;
//                        return true;
//                    }

//                    nanosTimeout = deadline - SbUtil.NanoTime();
//                    if (nanosTimeout <= 0L)
//                    {
//                        this.CancelAcquire(node);
//                        return false;
//                    }

//                    if (ShouldParkAfterFailedAcquire(p, node) && nanosTimeout > 1000L)
//                    {
//                        LockSupport.ParkNanos(nanosTimeout);
//                    }
//                } while (!Thread.interrupted());

//                throw new ThreadInterruptedException();
//            }
//            catch (Exception var8)
//            {
//                this.CancelAcquire(node);
//                throw var8;
//            }
//        }

//        private void DoAcquireShared(int arg)
//        {
//            AbstractQueuedSynchronizer.Node node = this.AddWaiter(AbstractQueuedSynchronizer.Node.SHARED);
//            bool interrupted = false;

//            try
//            {
//                while (true)
//                {
//                    AbstractQueuedSynchronizer.Node p = node.Predecessor();
//                    if (p == this.head)
//                    {
//                        int r = this.TryAcquireShared(arg);
//                        if (r >= 0)
//                        {
//                            this.SetHeadAndPropagate(node, r);
//                            p.next = null;
//                            return;
//                        }
//                    }

//                    if (ShouldParkAfterFailedAcquire(p, node))
//                    {
//                        interrupted |= this.ParkAndCheckInterrupt();
//                    }
//                }
//            }
//            catch (Exception var9)
//            {
//                this.CancelAcquire(node);
//                throw var9;
//            }
//            finally
//            {
//                if (interrupted)
//                {
//                    SelfInterrupt();
//                }

//            }
//        }

//        private void DoAcquireSharedInterruptibly(int arg)
//        {
//            AbstractQueuedSynchronizer.Node node = this.AddWaiter(AbstractQueuedSynchronizer.Node.SHARED);

//            try
//            {
//                AbstractQueuedSynchronizer.Node p;
//                do
//                {
//                    p = node.Predecessor();
//                    if (p == this.head)
//                    {
//                        int r = this.TryAcquireShared(arg);
//                        if (r >= 0)
//                        {
//                            this.SetHeadAndPropagate(node, r);
//                            p.next = null;
//                            return;
//                        }
//                    }
//                } while (!ShouldParkAfterFailedAcquire(p, node) || !this.ParkAndCheckInterrupt());

//                throw new ThreadInterruptedException();
//            }
//            catch (Exception e)
//            {
//                this.CancelAcquire(node);
//                throw e;
//            }
//        }

//        private bool DoAcquireSharedNanos(int arg, long nanosTimeout)
//        {
//            if (nanosTimeout <= 0L)
//            {
//                return false;
//            }
//            else
//            {
//                long deadline = SbUtil.NanoTime() + nanosTimeout;
//                AbstractQueuedSynchronizer.Node node = this.AddWaiter(AbstractQueuedSynchronizer.Node.SHARED);

//                try
//                {
//                    do
//                    {
//                        AbstractQueuedSynchronizer.Node p = node.Predecessor();
//                        if (p == this.head)
//                        {
//                            int r = this.TryAcquireShared(arg);
//                            if (r >= 0)
//                            {
//                                this.SetHeadAndPropagate(node, r);
//                                p.next = null;
//                                return true;
//                            }
//                        }

//                        nanosTimeout = deadline - SbUtil.NanoTime();
//                        if (nanosTimeout <= 0L)
//                        {
//                            this.CancelAcquire(node);
//                            return false;
//                        }

//                        if (ShouldParkAfterFailedAcquire(p, node) && nanosTimeout > 1000L)
//                        {
//                            LockSupport.ParkNanos(nanosTimeout);
//                        }
//                    } while (!Thread.interrupted());

//                    throw new ThreadInterruptedException();
//                }
//                catch (Exception e)
//                {
//                    this.CancelAcquire(node);
//                    throw e;
//                }
//            }
//        }

//        protected bool TryAcquire(int arg)
//        {
//            throw new Exception();
//        }

//        protected bool tryRelease(int arg)
//        {
//            throw new Exception();
//        }

//        protected int TryAcquireShared(int arg)
//        {
//            throw new Exception();
//        }

//        protected bool tryReleaseShared(int arg)
//        {
//            throw new Exception();
//        }

//        protected bool IsHeldExclusively()
//        {
//            throw new Exception();
//        }

//        public void Acquire(int arg)
//        {
//            if (!this.TryAcquire(arg) && this.AcquireQueued(this.AddWaiter(AbstractQueuedSynchronizer.Node.EXCLUSIVE), arg))
//            {
//                SelfInterrupt();
//            }

//        }

//        public void AcquireInterruptibly(int arg)
//        {
//            if (Thread.interrupted())
//            {
//                throw new ThreadInterruptedException();
//            }
//            else
//            {
//                if (!this.TryAcquire(arg))
//                {
//                    this.DoAcquireInterruptibly(arg);
//                }

//            }
//        }

//        public bool TryAcquireNanos(int arg, long nanosTimeout)
//        {
//            if (Thread.interrupted())
//            {
//                throw new ThreadInterruptedException();
//            }
//            else
//            {
//                return this.TryAcquire(arg) || this.DoAcquireNanos(arg, nanosTimeout);
//            }
//        }

//        public bool Release(int arg)
//        {
//            if (this.tryRelease(arg))
//            {
//                AbstractQueuedSynchronizer.Node h = this.head;
//                if (h != null && h.waitStatus != 0)
//                {
//                    this.UnParkSuccessor(h);
//                }

//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public void AcquireShared(int arg)
//        {
//            if (this.TryAcquireShared(arg) < 0)
//            {
//                this.DoAcquireShared(arg);
//            }

//        }

//        public void AcquireSharedInterruptibly(int arg)
//        {
//            if (Thread.interrupted())
//            {
//                throw new ThreadInterruptedException();
//            }
//            else
//            {
//                if (this.TryAcquireShared(arg) < 0)
//                {
//                    this.DoAcquireSharedInterruptibly(arg);
//                }

//            }
//        }

//        public bool TryAcquireSharedNanos(int arg, long nanosTimeout)
//        {
//            if (Thread.interrupted())
//            {
//                throw new ThreadInterruptedException();
//            }
//            else
//            {
//                return this.TryAcquireShared(arg) >= 0 || this.DoAcquireSharedNanos(arg, nanosTimeout);
//            }
//        }

//        public bool ReleaseShared(int arg)
//        {
//            if (this.tryReleaseShared(arg))
//            {
//                this.DoReleaseShared();
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public bool HasQueuedThreads()
//        {
//            var p = this.tail;

//            for (var h = this.head; p != h && p != null; p = p.prev)
//            {
//                if (p.waitStatus <= 0)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        public bool HasContended()
//        {
//            return this.head != null;
//        }

//        public Thread GetFirstQueuedThread()
//        {
//            return this.head == this.tail ? null : this.FullGetFirstQueuedThread();
//        }

//        private Thread FullGetFirstQueuedThread()
//        {
//            Node h;
//            Node s;
//            Thread st;
//            if (((h = this.head) == null || (s = h.next) == null || s.prev != this.head || (st = s.thread) == null) && ((h = this.head) == null || (s = h.next) == null || s.prev != this.head || (st = s.thread) == null))
//            {
//                Thread firstThread = null;

//                for (var p = this.tail; p != null && p != this.head; p = p.prev)
//                {
//                    var t = p.thread;
//                    if (t != null)
//                    {
//                        firstThread = t;
//                    }
//                }

//                return firstThread;
//            }
//            else
//            {
//                return st;
//            }
//        }

//        public bool IsQueued(Thread thread)
//        {
//            if (thread == null)
//            {
//                throw new NullReferenceException();
//            }
//            else
//            {
//                for (var p = this.tail; p != null; p = p.prev)
//                {
//                    if (p.thread == thread)
//                    {
//                        return true;
//                    }
//                }

//                return false;
//            }
//        }

//        public bool ApparentlyFirstQueuedIsExclusive()
//        {
//            Node h;
//            Node s;
//            return (h = this.head) != null && (s = h.next) != null && !s.IsShared() && s.thread != null;
//        }

//        public bool HasQueuedPredecessors()
//        {
//            Node h;
//            if ((h = this.head) != null)
//            {
//                Node s;
//                if ((s = h.next) == null || s.waitStatus > 0)
//                {
//                    s = null;

//                    for (var p = this.tail; p != h && p != null; p = p.prev)
//                    {
//                        if (p.waitStatus <= 0)
//                        {
//                            s = p;
//                        }
//                    }
//                }

//                if (s != null && s.thread != Thread.CurrentThread)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        public int GetQueueLength()
//        {
//            int n = 0;

//            for (var p = this.tail; p != null; p = p.prev)
//            {
//                if (p.thread != null)
//                {
//                    ++n;
//                }
//            }

//            return n;
//        }

//        public ICollection<Thread> GetQueuedThreads()
//        {
//            var list = new List<Thread>();

//            for (AbstractQueuedSynchronizer.Node p = this.tail; p != null; p = p.prev)
//            {
//                Thread t = p.thread;
//                if (t != null)
//                {
//                    list.Add(t);
//                }
//            }

//            return list;
//        }

//        public ICollection<Thread> GetExclusiveQueuedThreads()
//        {
//            var list = new List<Thread>();

//            for (AbstractQueuedSynchronizer.Node p = this.tail; p != null; p = p.prev)
//            {
//                if (!p.IsShared())
//                {
//                    Thread t = p.thread;
//                    if (t != null)
//                    {
//                        list.Add(t);
//                    }
//                }
//            }

//            return list;
//        }

//        public ICollection<Thread> GetSharedQueuedThreads()
//        {
//            var list = new List<Thread>();

//            for (AbstractQueuedSynchronizer.Node p = this.tail; p != null; p = p.prev)
//            {
//                if (p.IsShared())
//                {
//                    Thread t = p.thread;
//                    if (t != null)
//                    {
//                        list.Add(t);
//                    }
//                }
//            }

//            return list;
//        }

//        public override string ToString()
//        {
//            return base.ToString() + "[State = " + ", " + (this.HasQueuedThreads() ? "non" : "") + "empty queue]";
//        }

//        public bool IsOnSyncQueue(AbstractQueuedSynchronizer.Node node)
//        {
//            if (node.waitStatus != -2 && node.prev != null)
//            {
//                return node.next != null ? true : this.FindNodeFromTail(node);
//            }
//            else
//            {
//                return false;
//            }
//        }

//        private bool FindNodeFromTail(AbstractQueuedSynchronizer.Node node)
//        {
//            for (AbstractQueuedSynchronizer.Node p = this.tail; p != node; p = p.prev)
//            {
//                if (p == null)
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        public bool TransferForSignal(AbstractQueuedSynchronizer.Node node)
//        {
//            if (!node.CompareAndSetWaitStatus(-2, 0))
//            {
//                return false;
//            }
//            else
//            {
//                AbstractQueuedSynchronizer.Node p = this.Enq(node);
//                int ws = p.waitStatus;
//                if (ws > 0 || !p.CompareAndSetWaitStatus(ws, -1))
//                {
//                    LockSupport.UnPark(node.thread);
//                }

//                return true;
//            }
//        }

//        private bool TransferAfterCancelledWait(AbstractQueuedSynchronizer.Node node)
//        {
//            if (node.CompareAndSetWaitStatus(-2, 0))
//            {
//                this.Enq(node);
//                return true;
//            }
//            else
//            {
//                while (!this.IsOnSyncQueue(node))
//                {
//                    Thread.Yield();
//                }

//                return false;
//            }
//        }

//        private int FullyRelease(AbstractQueuedSynchronizer.Node node)
//        {
//            try
//            {
//                int savedState = this.GetState();
//                if (this.Release(savedState))
//                {
//                    return savedState;
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception var3)
//            {
//                node.waitStatus = 1;
//                throw var3;
//            }
//        }

//        public bool Owns(AbstractQueuedSynchronizer.ConditionObject condition)
//        {
//            return condition.IsOwnedBy(this);
//        }

//        public bool HasWaiters(AbstractQueuedSynchronizer.ConditionObject condition)
//        {
//            if (!this.Owns(condition))
//            {
//                throw new Exception("Not owner");
//            }
//            else
//            {
//                return condition.HasWaiters();
//            }
//        }

//        public int GetWaitQueueLength(ConditionObject condition)
//        {
//            if (!this.Owns(condition))
//            {
//                throw new Exception();
//            }
//            else
//            {
//                return condition.GetWaitQueueLength();
//            }
//        }

//        public ICollection<Thread> GetWaitingThreads(ConditionObject condition)
//        {
//            if (!this.Owns(condition))
//            {
//                throw new Exception();
//            }
//            else
//            {
//                return condition.GetWaitingThreads();
//            }
//        }

//        private void InitializeSyncQueue()
//        {
//            Node h;
//            if (Interlocked.CompareExchange(ref this.head,  h=new Node(), null) == null)
//            {
//                this.tail = h;
//            }
//        }

//        private bool CompareAndSetTail(AbstractQueuedSynchronizer.Node expect, AbstractQueuedSynchronizer.Node update)
//        {
//            return Interlocked.CompareExchange(ref this.tail, update, expect) == expect;
//        }



//        [Serializable]
//        public class ConditionObject : ICondition
//        {
//            private AbstractQueuedSynchronizer abstractQueuedSynchronizer;

//            private Node firstWaiter;
//            private Node lastWaiter;
//            private static readonly int REINTERRUPT = 1;
//            private static readonly int THROW_IE = -1;

//            public ConditionObject(AbstractQueuedSynchronizer abstractQueuedSynchronizer)
//            {
//                this.abstractQueuedSynchronizer = abstractQueuedSynchronizer;
//            }

//            private Node AddConditionWaiter()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    Node t = this.lastWaiter;
//                    if (t != null && t.waitStatus != -2)
//                    {
//                        this.UnlinkCancelledWaiters();
//                        t = this.lastWaiter;
//                    }

//                    Node node = new Node(-2);
//                    if (t == null)
//                    {
//                        this.firstWaiter = node;
//                    }
//                    else
//                    {
//                        t.nextWaiter = node;
//                    }

//                    this.lastWaiter = node;
//                    return node;
//                }
//            }

//            private void DoSignal(Node first)
//            {
//                do
//                {
//                    if ((this.firstWaiter = first.nextWaiter) == null)
//                    {
//                        this.lastWaiter = null;
//                    }

//                    first.nextWaiter = null;
//                } while (!abstractQueuedSynchronizer.TransferForSignal(first) && (first = this.firstWaiter) != null);

//            }

//            private void DoSignalAll(Node first)
//            {
//                this.lastWaiter = this.firstWaiter = null;

//                Node next;
//                do
//                {
//                    next = first.nextWaiter;
//                    first.nextWaiter = null;
//                    abstractQueuedSynchronizer.TransferForSignal(first);
//                    first = next;
//                } while (next != null);

//            }

//            private void UnlinkCancelledWaiters()
//            {
//                Node t = this.firstWaiter;

//                Node next;
//                for (Node trail = null; t != null; t = next)
//                {
//                    next = t.nextWaiter;
//                    if (t.waitStatus != -2)
//                    {
//                        t.nextWaiter = null;
//                        if (trail == null)
//                        {
//                            this.firstWaiter = next;
//                        }
//                        else
//                        {
//                            trail.nextWaiter = next;
//                        }

//                        if (next == null)
//                        {
//                            this.lastWaiter = trail;
//                        }
//                    }
//                    else
//                    {
//                        trail = t;
//                    }
//                }

//            }

//            public void Signal()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    Node first = this.firstWaiter;
//                    if (first != null)
//                    {
//                        this.DoSignal(first);
//                    }

//                }
//            }

//            public void SignalAll()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    Node first = this.firstWaiter;
//                    if (first != null)
//                    {
//                        this.DoSignalAll(first);
//                    }

//                }
//            }

//            public void AwaitUninterruptibly()
//            {
//                Node node = this.AddConditionWaiter();
//                int savedState = abstractQueuedSynchronizer.FullyRelease(node);
//                bool interrupted = false;

//                while (!abstractQueuedSynchronizer.IsOnSyncQueue(node))
//                {
//                    LockSupport.Park();
//                    Thread.CurrentThread.Interrupt();
//                    interrupted = true;
//                }

//                if (abstractQueuedSynchronizer.AcquireQueued(node, savedState) || interrupted)
//                {
//                    AbstractQueuedSynchronizer.SelfInterrupt();
//                }

//            }

//            private int CheckInterruptWhileWaiting(Node node)
//            {
//                return Thread.interrupted() ? (abstractQueuedSynchronizer.TransferAfterCancelledWait(node) ? -1 : 1) : 0;
//            }

//            private void ReportInterruptAfterWait(int interruptMode)
//            {
//                if (interruptMode == -1)
//                {
//                    throw new ThreadInterruptedException();
//                }
//                else
//                {
//                    if (interruptMode == 1)
//                    {
//                        AbstractQueuedSynchronizer.SelfInterrupt();
//                    }

//                }
//            }

//            public void Await()
//            {
//                if (Thread.interrupted())
//                {
//                    throw new ThreadInterruptedException();
//                }
//                else
//                {
//                    Node node = this.AddConditionWaiter();
//                    int savedState = abstractQueuedSynchronizer.FullyRelease(node);
//                    int interruptMode = 0;

//                    while (!abstractQueuedSynchronizer.IsOnSyncQueue(node))
//                    {
//                        LockSupport.Park();
//                        if ((interruptMode = this.CheckInterruptWhileWaiting(node)) != 0)
//                        {
//                            break;
//                        }
//                    }

//                    if (abstractQueuedSynchronizer.AcquireQueued(node, savedState) && interruptMode != -1)
//                    {
//                        interruptMode = 1;
//                    }

//                    if (node.nextWaiter != null)
//                    {
//                        this.UnlinkCancelledWaiters();
//                    }

//                    if (interruptMode != 0)
//                    {
//                        this.ReportInterruptAfterWait(interruptMode);
//                    }

//                }
//            }

//            public long AwaitNanos(long nanosTimeout)
//            {
//                if (Thread.interrupted())
//                {
//                    throw new ThreadInterruptedException();
//                }
//                else
//                {
//                    long deadline = SbUtil.NanoTime() + nanosTimeout;
//                    Node node = this.AddConditionWaiter();
//                    int savedState = abstractQueuedSynchronizer.FullyRelease(node);

//                    int interruptMode;
//                    for (interruptMode = 0; !abstractQueuedSynchronizer.IsOnSyncQueue(node); nanosTimeout = deadline - SbUtil.NanoTime())
//                    {
//                        if (nanosTimeout <= 0L)
//                        {
//                            abstractQueuedSynchronizer.TransferAfterCancelledWait(node);
//                            break;
//                        }

//                        if (nanosTimeout > 1000L)
//                        {
//                            LockSupport.ParkNanos(nanosTimeout);
//                        }

//                        if ((interruptMode = this.CheckInterruptWhileWaiting(node)) != 0)
//                        {
//                            break;
//                        }
//                    }

//                    if (abstractQueuedSynchronizer.AcquireQueued(node, savedState) && interruptMode != -1)
//                    {
//                        interruptMode = 1;
//                    }

//                    if (node.nextWaiter != null)
//                    {
//                        this.UnlinkCancelledWaiters();
//                    }

//                    if (interruptMode != 0)
//                    {
//                        this.ReportInterruptAfterWait(interruptMode);
//                    }

//                    long remaining = deadline - SbUtil.NanoTime();
//                    return remaining <= nanosTimeout ? remaining : -9223372036854775808L;
//                }
//            }

//            public bool AwaitUntil(DateTime deadline)
//            {
//                long abstime = deadline.getTime();
//                if (Thread.interrupted())
//                {
//                    throw new ThreadInterruptedException();
//                }
//                else
//                {
//                    Node node = this.AddConditionWaiter();
//                    int savedState = abstractQueuedSynchronizer.FullyRelease(node);
//                    bool timedout = false;
//                    int interruptMode = 0;

//                    while (!abstractQueuedSynchronizer.IsOnSyncQueue(node))
//                    {
//                        if (SbUtil.CurrentTimeMillis() >= abstime)
//                        {
//                            timedout = abstractQueuedSynchronizer.TransferAfterCancelledWait(node);
//                            break;
//                        }

//                        LockSupport.ParkUntil(abstime);
//                        if ((interruptMode = this.CheckInterruptWhileWaiting(node)) != 0)
//                        {
//                            break;
//                        }
//                    }

//                    if (abstractQueuedSynchronizer.AcquireQueued(node, savedState) && interruptMode != -1)
//                    {
//                        interruptMode = 1;
//                    }

//                    if (node.nextWaiter != null)
//                    {
//                        this.UnlinkCancelledWaiters();
//                    }

//                    if (interruptMode != 0)
//                    {
//                        this.ReportInterruptAfterWait(interruptMode);
//                    }

//                    return !timedout;
//                }
//            }

//            public bool Await(long time, TimeSpan unit)
//            {
//                long nanosTimeout = unit.toNanos(time);
//                if (Thread.interrupted())
//                {
//                    throw new ThreadInterruptedException();
//                }
//                else
//                {
//                    long deadline = SbUtil.NanoTime() + nanosTimeout;
//                    Node node = this.AddConditionWaiter();
//                    int savedState = abstractQueuedSynchronizer.FullyRelease(node);
//                    bool timedout = false;

//                    int interruptMode;
//                    for (interruptMode = 0; !abstractQueuedSynchronizer.IsOnSyncQueue(node); nanosTimeout = deadline - SbUtil.NanoTime())
//                    {
//                        if (nanosTimeout <= 0L)
//                        {
//                            timedout = abstractQueuedSynchronizer.TransferAfterCancelledWait(node);
//                            break;
//                        }

//                        if (nanosTimeout > 1000L)
//                        {
//                            LockSupport.ParkNanos(nanosTimeout);
//                        }

//                        if ((interruptMode = this.CheckInterruptWhileWaiting(node)) != 0)
//                        {
//                            break;
//                        }
//                    }

//                    if (abstractQueuedSynchronizer.AcquireQueued(node, savedState) && interruptMode != -1)
//                    {
//                        interruptMode = 1;
//                    }

//                    if (node.nextWaiter != null)
//                    {
//                        this.UnlinkCancelledWaiters();
//                    }

//                    if (interruptMode != 0)
//                    {
//                        this.ReportInterruptAfterWait(interruptMode);
//                    }

//                    return !timedout;
//                }
//            }

//            public bool IsOwnedBy(AbstractQueuedSynchronizer sync)
//            {
//                return sync == abstractQueuedSynchronizer;
//            }

//            public bool HasWaiters()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    for (Node w = this.firstWaiter; w != null; w = w.nextWaiter)
//                    {
//                        if (w.waitStatus == -2)
//                        {
//                            return true;
//                        }
//                    }

//                    return false;
//                }
//            }

//            public int GetWaitQueueLength()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    int n = 0;

//                    for (Node w = this.firstWaiter; w != null; w = w.nextWaiter)
//                    {
//                        if (w.waitStatus == -2)
//                        {
//                            ++n;
//                        }
//                    }

//                    return n;
//                }
//            }

//            public ICollection<Thread> GetWaitingThreads()
//            {
//                if (!abstractQueuedSynchronizer.IsHeldExclusively())
//                {
//                    throw new IllegalMonitorStateException();
//                }
//                else
//                {
//                    IList<Thread> list = new List<Thread>();

//                    for (Node w = this.firstWaiter; w != null; w = w.nextWaiter)
//                    {
//                        if (w.waitStatus == -2)
//                        {
//                            Thread t = w.thread;
//                            if (t != null)
//                            {
//                                list.Add(t);
//                            }
//                        }
//                    }

//                    return list;
//                }
//            }
//        }
//        public sealed class Node
//        {
//            public static Node SHARED = new Node();
//            public static Node EXCLUSIVE = null;

//            private readonly int CANCELLED = 1;
//            private readonly int SIGNAL = -1;
//            private readonly int CONDITION = -2;
//            private readonly int PROPAGATE = -3;
//            public int waitStatus;
//            public volatile Node prev;
//            public volatile Node next;
//            public Thread thread;
//            public Node nextWaiter;

//            public bool IsShared()
//            {
//                return this.nextWaiter == SHARED;
//            }

//            public Node Predecessor()
//            {
//                var p = this.prev;
//                if (p == null)
//                {
//                    throw new NullReferenceException();
//                }
//                else
//                {
//                    return p;
//                }
//            }

//            public Node()
//            {
//            }

//            public Node(Node nextWaiter)
//            {
//                this.nextWaiter = nextWaiter;
//                Interlocked.Exchange(ref thread, Thread.CurrentThread);
//            }
//            public Node(int waitStatus)
//            {
//                Interlocked.Exchange(ref waitStatus, waitStatus);
//                Interlocked.Exchange(ref thread, Thread.CurrentThread);
//            }

//            public bool CompareAndSetWaitStatus(int expect, int update)
//            {
//                return Interlocked.CompareExchange(ref waitStatus, update, expect) == expect;
//            }
//            public bool CompareAndSetNext(Node expect, Node update)
//            {
//                return Interlocked.CompareExchange(ref next, update, expect) == expect;
//            }

//            public void SetPrevRelaxed(Node p)
//            {
//                Interlocked.Exchange(ref prev, p);
//            }
//        }

//    }
//}