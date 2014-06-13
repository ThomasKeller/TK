using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace TK.KostalPlugin
{
    public class KostalJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine(string.Format("Hello World! - {0} {1}",
                System.DateTime.Now.ToString("r"),
                System.Threading.Thread.CurrentThread.ManagedThreadId));
            System.Random random = new Random();
            int sleep = random.Next(0, 5000);
            System.Threading.Thread.Sleep(sleep);
            Console.WriteLine(string.Format("Goodbye! - {0} {1}",
                System.DateTime.Now.ToString("r"),
                System.Threading.Thread.CurrentThread.ManagedThreadId));

        }
    }
}
