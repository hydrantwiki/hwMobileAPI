using Mono.Unix;
using Mono.Unix.Native;
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

                if (Type.GetType("Mono.Runtime") != null)
                {
                    // on mono, processes will usually run as daemons - this allows you to listen
                    // for termination signals (ctrl+c, shutdown, etc) and finalize correctly
                    UnixSignal.WaitAny(new[] {
                            new UnixSignal(Signum.SIGINT),
                            new UnixSignal(Signum.SIGTERM),
                            new UnixSignal(Signum.SIGQUIT),
                            new UnixSignal(Signum.SIGHUP)
                    });
                } else {
                    Console.ReadKey();
                }                             

                TraceFileHelper.Info("Stopping webserver");
                host.Stop();
            }
        }
    }
}
