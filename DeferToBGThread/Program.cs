using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeferToBGThread
{

    class LLA
    {
        public LLA() { }

        public void doSave(TestOb thrd)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, -- enter doSave", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            Thread.Sleep(TimeSpan.FromSeconds(thrd.saveWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    leaving doSave",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            return;
        }

        public int doGet(TestOb tobj)
        {
            Console.WriteLine("Thread: {0}, Time: {1}, ++ enter doGet", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now);

            Thread.Sleep(TimeSpan.FromSeconds(tobj.getWaitTime));

            Console.WriteLine("Thread: {0}, Time: {1},    doGet returning {2}", 
                Thread.CurrentThread.ManagedThreadId,
                DateTime.Now,
                tobj.id);

            return tobj.id;
        }
    }

    class TestOb
    {
        int initialWaitTime;
        public int id;
        public int saveWaitTime;
        public int getWaitTime;
        LLA lla;

        public TestOb(int id, int initialWaitTime, int saveTime, int getWaitTime, LLA lla)
        {
            this.id = id;
            this.initialWaitTime = initialWaitTime;
            this.saveWaitTime = saveTime;
            this.getWaitTime = getWaitTime;
            this.lla = lla;
        }

        public void run()
        {
            Thread.Sleep(TimeSpan.FromSeconds(initialWaitTime));
            lla.doSave(this);
            lla.doGet(this);
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

            List<Thread> thrds = new List<Thread>();
            foreach (TestOb tobj in obs)
            {
                tobj.run();
                //thrds.Add(new Thread(new ThreadStart(tobj.run)));
            }
        }
    }
}
