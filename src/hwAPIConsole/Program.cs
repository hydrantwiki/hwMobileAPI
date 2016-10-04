using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwAPIConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new NancyHost(new Uri("http://localhost:8990")))
            {
                host.Start();

                Console.ReadKey();
                host.Stop();
            }
        }
    }
}
