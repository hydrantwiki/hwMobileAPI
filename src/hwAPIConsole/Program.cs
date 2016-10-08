using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGecko.Library.Common.Helpers;

namespace hwAPIConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TraceFileHelper.SetupLogging();
            Trace.Listeners.Add(new ConsoleTraceListener());

            using (var host = new NancyHost(new Uri("http://localhost:8990")))
            {
                host.Start();
                TraceFileHelper.Info("Started webserver");
                
                Console.ReadKey();
                host.Stop();
            }
        }
    }
}
