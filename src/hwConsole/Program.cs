using HydrantWiki.Library.Enums;
using HydrantWiki.Library.Helpers;
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
                    if (args.Length > 1)
                    {
                        DumpHydrants(args[0]);
                    }
                }
                else if (action.Equals("calcleaders"))
                {
                    CalcLeaders();
                }
            }
        }

        static void DumpHydrants(string filename)
        {
            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            string body = JsonConvert.SerializeObject(hydrants);

            //TODO Send to S3
            File.WriteAllText(filename, body);

        }

        static void CalcLeaders(DateTime? _now = null)
        {
            DateTime now;
            if (_now == null)
            {
                now = DateTime.UtcNow;
            }
            else
            {
                now = _now.Value;
            }
            
            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            Dictionary<Guid, User> users = manager.GetHydrantWikiUsers();
            List<UserStats> final = users.Values.ToList().RebuildStats(now);
            
            DailyStanding standing = manager.GetDailyStanding(now);
            if (standing == null)
            {
                standing = new DailyStanding();
                standing.Active = true;
                standing.Date = now.Date;
                standing.Guid = Guid.NewGuid();
                standing.LastModifiedDateTime = now;
            }
            standing.ActiveUsers = final.Count;

            //Sort and build daily standings based on new hydrants
            List<Place> places = final.DeterminePlaces(UserStatSortColumn.NewHydrant);
            Dictionary<Guid, DailyStandingUser> dsus = places.DetermineStanding(standing.Guid);

            places = final.DeterminePlaces(UserStatSortColumn.ApprovedTags);
            dsus = places.DetermineStanding(standing.Guid, dsus);

            manager.Persist(final);
            manager.Persist(standing);
            manager.DeleteUserStandings(standing.Guid);
            manager.Persist(dsus.Values.ToList());
        }
    }
}
