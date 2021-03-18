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
        public int data;
        public int id;

        public HWTestData(int id, int saveTime, int getWaitTime)
        {
            this.data = int.MinValue;
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
            Console.WriteLine("Thread: {0}, Time: {1}, -- enter doSave     - id = {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            Thread.Sleep(TimeSpan.FromSeconds(td.saveWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    leaving doSave   - id = {2}",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            return;
        }

        public int doGet(HWTestData td)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, ++ enter doGet   - id = {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.id);

            Thread.Sleep(TimeSpan.FromSeconds(td.getWaitTime));
            td.data = td.id + 10;

            Console.WriteLine("Thread: {0}, Time: {1},    doGet         - returning {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                td.data);

            return td.id;
        }
    }

    class TestOb
    {
        int measExecTime;
        HWTestData td;
        LLA lla;

        public ManualResetEvent releaseHwEvent = new ManualResetEvent(false);

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
            lla.doGet(td);
            releaseHwEvent.Set();
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
                Console.WriteLine("Created thread {0}", tstThread.ManagedThreadId);
                tstThread.Start();
                tobj.releaseHwEvent.WaitOne();
                tobj.releaseHwEvent.Reset();
            }
        }
    }
}
