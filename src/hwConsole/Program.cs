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
            DateTime now = DateTime.UtcNow;

            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            Dictionary<Guid, User> users = manager.GetHydrantWikiUsers();

            Dictionary<Guid, int> hydrantCountByUser = manager.GetNewHydrantsByUser();
            Dictionary<Guid, TagStats> statsByUser = manager.GetTagStatsByUser();

            List<UserStats> final = new List<UserStats>();

            //Update the user stats
            foreach (var userGuid in users.Keys)
            {
                UserStats userStats = manager.GetUserStats(userGuid);

                if (userStats == null)
                {
                    userStats = new UserStats();
                    userStats.Active = true;
                    userStats.Guid = Guid.NewGuid();
                    userStats.LastModifiedBy = null;
                    userStats.LastModifiedDateTime = now;
                    userStats.ParentGuid = null;
                    userStats.UserGuid = userGuid;                    
                }

                if (hydrantCountByUser.ContainsKey(userGuid))
                {
                    userStats.NewHydrantsTagged = hydrantCountByUser[userGuid];                    
                } else
                {
                    userStats.NewHydrantsTagged = 0;
                }

                if (statsByUser.ContainsKey(userGuid))
                {
                    var tagStat = statsByUser[userGuid];
                    userStats.ApprovedTagCount = tagStat.ApprovedTagCount;
                    userStats.PendingTagCount = tagStat.PendingTagCount;
                    userStats.RejectedTagCount = tagStat.RejectedTagCount;
                } else
                {
                    userStats.ApprovedTagCount = 0;
                    userStats.PendingTagCount = 0;
                    userStats.RejectedTagCount = 0;
                }

                manager.Persist(userStats);

                final.Add(userStats);
            }

            //Sort and build daily standing
            List<UserStats> sorted = final.OrderByDescending(u => u.NewHydrantsTagged).ToList();
            List<Place> places = new List<Place>();

            Place currentPlace = new Place();
            currentPlace.Score = 0;

            //get the user stats into buckets
            foreach (var userStat in sorted)
            {
                if (userStat.NewHydrantsTagged > 0)
                {
                    if (userStat.NewHydrantsTagged == currentPlace.Score)
                    {
                        currentPlace.UsersAtThisPosition.Add(userStat);
                    }
                    else
                    {
                        currentPlace = new Place();
                        currentPlace.Score = userStat.NewHydrantsTagged;
                        currentPlace.UsersAtThisPosition.Add(userStat);
                        places.Add(currentPlace);
                    }
                }
            }

            DailyStanding standing = manager.GetDailyStanding(now);
            if (standing == null)
            {
                standing = new DailyStanding();
                standing.Active = true;
                standing.Date = now.Date;
                standing.Guid = Guid.NewGuid();
                standing.LastModifiedDateTime = now;
            }

            int position = 1;
            foreach (var place in places)
            {
                foreach (var userStat in place.UsersAtThisPosition)
                {
                                        
                }

                position += place.UsersAtThisPosition.Count;
            }



        }
    }
}
