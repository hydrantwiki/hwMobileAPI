using System.Collections.Generic;

namespace HydrantWiki.Mobile.Api.Objects
{
    public class TagToReview : Tag
    {
        public string Username { get; set; }

        public int UserTagsApproved { get; set; }

        public int UserTagsRejected { get; set; }

        public string Status { get; set; }

        public List<HydrantHeader> NearbyHydrants { get; set; }
    }
}