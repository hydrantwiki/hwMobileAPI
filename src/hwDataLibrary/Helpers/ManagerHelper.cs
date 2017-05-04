using HydrantWiki.Library.Constants;
using HydrantWiki.Library.Managers;
using HydrantWiki.Library.Objects;
using System;
using System.Collections.Generic;

namespace HydrantWiki.Library.Helpers
{
    public static class ManagerHelper
    {
        public static Dictionary<Guid, User> GetHydrantWikiUsers(this HydrantWikiManager _manager)
        {
            List<User> users = _manager.GetUsers();

            Dictionary<Guid, User> output = new Dictionary<Guid, User>();

            foreach (var user in users)
            {
                if (user.UserSource == UserSources.HydrantWiki)
                {
                    output.Add(user.Guid, user);
                }
            }

            return output;
        }

        public static Dictionary<Guid, int> GetNewHydrantsByUser(this HydrantWikiManager _manager)
        {
            HydrantWikiManager manager = new HydrantWikiManager();
            List<Hydrant> hydrants = manager.GetHydrants();

            Dictionary<Guid, User> users = manager.GetHydrantWikiUsers();

            Dictionary<Guid, int> hydrantCountByUser = new Dictionary<Guid, int>();

            foreach (var hydrant in hydrants)
            {
                Guid userGuid = hydrant.OriginalTagUserGuid;

                if (users.ContainsKey(userGuid))
                {
                    int count = 0;

                    if (hydrantCountByUser.ContainsKey(userGuid))
                    {
                        count = hydrantCountByUser[userGuid];
                    }

                    count++;

                    hydrantCountByUser[userGuid] = count;
                }
            }

            return hydrantCountByUser;
        }

        public static Dictionary<Guid, TagStats> GetTagStatsByUser(this HydrantWikiManager _manager)
        {
            HydrantWikiManager manager = new HydrantWikiManager();
            Dictionary<Guid, User> users = manager.GetHydrantWikiUsers();

            Dictionary<Guid, TagStats> output = new Dictionary<Guid, TagStats>();

            foreach(var userGuid in users.Keys)
            {
                List<Tag> tags = manager.GetTagsForUser(userGuid);

                int tagCount = tags.Count;
                int tagsRejected = 0;
                int tagsApproved = 0;
                int tagsPending = 0;

                foreach (var tag in tags)
                {
                    if (tag.Status == TagStatus.Approved)
                    {
                        tagsApproved++;
                    } else if (tag.Status == TagStatus.Pending)
                    {
                        tagsPending++;
                    } else if (tag.Status == TagStatus.Rejected)
                    {
                        tagsRejected++;
                    }
                }

                TagStats stats = new TagStats();
                stats.ApprovedTagCount = tagsApproved;
                stats.PendingTagCount = tagsPending;
                stats.RejectedTagCount = tagsRejected;

                output.Add(userGuid, stats);
            }

            return output;
        }
    }
}
