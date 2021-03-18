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
        LLA() { }

    }

    class TThread
    {
        private int id;
        private int saveTime;
        private int getWaitTime;

        TThread(int id, int saveTime, int getWaitTime)
        {
            this.id = id;
            this.saveTime = saveTime;
            this.getWaitTime = getWaitTime;
        }

        void run()
        {
            Thread.Sleep(saveTime);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
