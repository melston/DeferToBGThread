using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace DeferToBGThread
{
    class HWTestData
    {
        public int saveWaitTime;
        public int getWaitTime;
        public int id;
        public int data;
        public ManualResetEvent uploadDoneEvent = new ManualResetEvent(false);

        public HWTestData(int id, int saveTime, int getWaitTime)
        {
            data = int.MinValue;
            this.id = id;
            this.saveWaitTime = saveTime;
            this.getWaitTime = getWaitTime;
        }
    }

    class LLA
    {
        BehaviorSubject<HWTestData> saves = new BehaviorSubject<HWTestData>(null);

        public LLA() 
        {
            saves
                .ObserveOn(new EventLoopScheduler(ts => new Thread(ts)))
                .Subscribe(handleSave);
        }

        void handleSave(HWTestData td)
        {
            if (td == null) return;

            Console.WriteLine("Thread: {0}, Time: {1}, -- enter doSave   - id = {2}",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            Thread.Sleep(TimeSpan.FromSeconds(td.saveWaitTime));

            td.data = td.id + 10;

            // Now let clients of the HWTestData know that the upload is done.
            td.uploadDoneEvent.Set();
 
            Console.WriteLine("Thread: {0}, Time: {1},    leaving doSave - id = {2}",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);
        }

        public void doSave(HWTestData td)
        {
            saves.OnNext(td);
            return;
        }

        public int doGet(HWTestData td)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, ++ enter doGet - id = {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            ///////////////////////////////////////////////////////////////////////////
            /// This block is a 'shim' to marry Rx and 'standard' threading so that
            /// we wait for an observable to have data available before proceeding
            /// in normal thread-based code.
            /// 
            /// Wait until the upload is done
            td.uploadDoneEvent.WaitOne();

            // Safety.  Make sure the observable reports false for the next time it
            // is subscribed to.
            td.uploadDoneEvent.Reset();

            Thread.Sleep(TimeSpan.FromSeconds(td.getWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    doGet returning {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.data);

            return td.data;
        }
    }

    class TestOb
    {
        int measExecTime;
        HWTestData td;
        LLA lla;

        public ManualResetEvent releaseHWEvent = new ManualResetEvent(false);

        public TestOb(int id, int measExecTime, int saveTime, int getWaitTime, LLA lla)
        {
            td = new HWTestData(id, saveTime, getWaitTime);

            this.measExecTime = measExecTime;
            this.lla = lla;
        }

        public void run()
        {
            Thread.Sleep(TimeSpan.FromSeconds(measExecTime));
            lla.doSave(td);
            releaseHWEvent.Set();
            lla.doGet(td);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LLA lla = new LLA();
            TestOb[] obs = new TestOb[] { 
                new TestOb(1, 1, 2, 1, lla),
                new TestOb(2, 1, 2, 1, lla),
                new TestOb(3, 1, 2, 1, lla),
                new TestOb(4, 1, 2, 1, lla),
            };

            List<Thread> threads = new List<Thread>();
            foreach (TestOb tobj in obs)
            {
                Thread tstThread = new Thread(tobj.run);
                threads.Add(tstThread);
                Console.WriteLine("Created thread {0}", tstThread.ManagedThreadId);
                tstThread.Start();
                tobj.releaseHWEvent.WaitOne();
                tobj.releaseHWEvent.Reset();
            }

            Console.ReadKey();
        }
    }
}
