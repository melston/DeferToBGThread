using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeferToBGThread
{
    class HWTestData
    {
        public int saveWaitTime;
        public int getWaitTime;
        public int id;

        public HWTestData(int id, int saveTime, int getWaitTime)
        {
            this.id = id;
            this.saveWaitTime = saveTime;
            this.getWaitTime = getWaitTime;
        }
    }

    class LLA
    {
        public LLA() { }

        public void doSave(HWTestData td)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, -- enter doSave", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            Thread.Sleep(TimeSpan.FromSeconds(td.saveWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    leaving doSave",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            return;
        }

        public int doGet(HWTestData td)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, ++ enter doGet", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            Thread.Sleep(TimeSpan.FromSeconds(td.getWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    doGet returning {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            return td.id;
        }
    }

    class TestOb
    {
        int initialWaitTime;
        HWTestData td;
        LLA lla;

        public ManualResetEvent tstDoneEvent = new ManualResetEvent(false);

        public TestOb(int id, int initialWaitTime, int saveTime, int getWaitTime, LLA lla)
        {
            td = new HWTestData(id, saveTime, getWaitTime);

            this.initialWaitTime = initialWaitTime;
            this.lla = lla;
        }

        public void run()
        {
            Thread.Sleep(TimeSpan.FromSeconds(initialWaitTime));
            lla.doSave(td);
            lla.doGet(td);
            tstDoneEvent.Set();
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

            foreach (TestOb tobj in obs)
            {
                Thread tstThread = new Thread(tobj.run);
                tstThread.Start();
                tobj.tstDoneEvent.WaitOne();
                tobj.tstDoneEvent.Reset();
            }
        }
    }
}
