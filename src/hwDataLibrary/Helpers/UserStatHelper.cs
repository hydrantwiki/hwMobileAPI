using HydrantWiki.Library.Enums;
using HydrantWiki.Library.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using HydrantWiki.Library.Managers;

namespace HydrantWiki.Library.Helpers
{
    public static class UserStatHelper
    {
        public static List<UserStats> RebuildStats(this List<User> _users, DateTime? _now = null)
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
            List<UserStats> result = new List<UserStats>();

            Dictionary<Guid, int> hydrantCountByUser = manager.GetNewHydrantsByUser();
            Dictionary<Guid, TagStats> statsByUser = manager.GetTagStatsByUser();

            //Update the user stats
            foreach (var user in _users)
            {
                UserStats userStats = manager.GetUserStats(user.Guid);

                if (userStats == null)
                {
                    userStats = new UserStats();
                    userStats.Active = true;
                    userStats.Guid = Guid.NewGuid();
                    userStats.LastModifiedBy = null;
                    userStats.LastModifiedDateTime = now;
                    userStats.ParentGuid = null;
                    userStats.UserGuid = user.Guid;
                    userStats.DisplayName = user.DisplayName;
                }
                else
                {
                    userStats.LastModifiedBy = null;
                    userStats.LastModifiedDateTime = now;
                    userStats.DisplayName = user.DisplayName;
                }

                if (hydrantCountByUser.ContainsKey(user.Guid))
                {
                    userStats.NewHydrantsTagged = hydrantCountByUser[user.Guid];
                }
                else
                {
                    userStats.NewHydrantsTagged = 0;
                }

                if (statsByUser.ContainsKey(user.Guid))
                {
                    var tagStat = statsByUser[user.Guid];
                    userStats.ApprovedTagCount = tagStat.ApprovedTagCount;
                    userStats.PendingTagCount = tagStat.PendingTagCount;
                    userStats.RejectedTagCount = tagStat.RejectedTagCount;
                }
                else
                {
                    userStats.ApprovedTagCount = 0;
                    userStats.PendingTagCount = 0;
                    userStats.RejectedTagCount = 0;
                }

                manager.Persist(userStats);

                result.Add(userStats);
            }

            return result;
        }

        public static int SortColumnValue(this UserStats _stat, UserStatSortColumn _sortBy)
        {
            if (_sortBy == UserStatSortColumn.NewHydrant)
            {
                return _stat.NewHydrantsTagged;
            }

            if (_sortBy == UserStatSortColumn.ApprovedTags)
            {
                return _stat.ApprovedTagCount;
            }

            if (_sortBy == UserStatSortColumn.RejectedTags)
            {
                return _stat.RejectedTagCount;
            }

            if (_sortBy == UserStatSortColumn.PendingTags)
            {
                return _stat.PendingTagCount;
            }

            throw new Exception("Unknown sortby column");
        }

        public static List<Place> DeterminePlaces(
            this List<UserStats> _stats, 
            UserStatSortColumn _sortBy)
        {
            List<UserStats> sorted = _stats.OrderByDescending(u => u.SortColumnValue(_sortBy)).ToList();
            List<Place> places = new List<Place>();

            Place currentPlace = new Place();
            currentPlace.Score = 0;

            //get the user stats based on hydrants
            foreach (var userStat in sorted)
            {
                var score = userStat.SortColumnValue(_sortBy);
                if (score == currentPlace.Score)
                {
                    currentPlace.UsersAtThisPosition.Add(userStat);
                }
                else
                {
                    if (score > 0)
                    {
                        currentPlace = new Place();
                        currentPlace.Score = userStat.SortColumnValue(_sortBy);
                        currentPlace.UsersAtThisPosition.Add(userStat);
                        currentPlace.SortedBy = _sortBy;
                        places.Add(currentPlace);
                    }
                }
            }
            
            return places;
        }

        public static Dictionary<Guid, DailyStandingUser> DetermineStanding(
            this List<Place> _places,
            Guid _dailyStandingGuid,
            Dictionary<Guid, DailyStandingUser> _standings = null)
        {
            if (_standings == null)
            {
                _standings = new Dictionary<Guid, DailyStandingUser>();
            }

            int position = 1;
            foreach (var place in _places)
            {
                foreach (var userStat in place.UsersAtThisPosition)
                {
                    DailyStandingUser dsu;
                    if (_standings.ContainsKey(userStat.UserGuid))
                    {
                        dsu = _standings[userStat.UserGuid];
                    }
                    else
                    {
                        dsu = new DailyStandingUser();
                        dsu.Guid = Guid.NewGuid();
                        dsu.UserGuid = userStat.UserGuid;
                        dsu.DisplayName = userStat.DisplayName;
                        dsu.ParentGuid = _dailyStandingGuid;
                        dsu.Active = true;
                        dsu.ApprovedTagPosition = 0;
                        dsu.ApprovedTagCount = 0;
                        dsu.NewHydrantPosition = 0;
                        dsu.NewHydrantCount = 0;

                        _standings.Add(dsu.UserGuid, dsu);
                    }

                    if (place.SortedBy == UserStatSortColumn.NewHydrant)
                    {
                        dsu.NewHydrantCount = place.Score;
                        dsu.NewHydrantPosition = position;
                    }
                    else if (place.SortedBy == UserStatSortColumn.ApprovedTags)
                    {
                        dsu.ApprovedTagCount = place.Score;
                        dsu.ApprovedTagPosition = position;
                    }
                }

                position += place.UsersAtThisPosition.Count;
            }


            return _standings;
        }
    }
}
