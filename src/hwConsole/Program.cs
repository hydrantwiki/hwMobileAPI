using HydrantWiki.Library.Managers;
using HydrantWiki.Library.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string action = args[0].ToLower();

                if (action.Equals("dump"))
                {
                    if (args.Length> 1)
                    {
                        DumpHydrants(args[0]);
                    }
                } else if (action.Equals("calcleaders"))
                {

                }
                



            }
            
        }

        static void DumpHydrants(string filename)
        {
            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            string body = JsonConvert.SerializeObject(hydrants);

            File.WriteAllText(filename, body);
        }

        static void CalcLeaders()
        {
            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            Dictionary<Guid, int> userCounts = new Dictionary<Guid, int>();

            foreach (var hydrant in hydrants)
            {
                Guid userGuid = hydrant.OriginalTagUserGuid;
                int count = 0;

                if (userCounts.ContainsKey(userGuid))
                {
                    count = userCounts[userGuid];
                }

                count++;

                userCounts[userGuid] = count;
            }



        }
    }
}
