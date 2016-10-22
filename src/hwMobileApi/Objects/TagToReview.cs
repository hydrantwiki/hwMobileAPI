using System.Collections.Generic;

namespace HydrantWiki.Mobile.Api.Objects
{
    public class TagToReview : Tag
    {
        public string Username { get; set; }

        public int UserTagsApproved { get; set; }

        public int UserTagsRejected { get; set; }

        public string Status { get; set; }

        public string ThumbnailUrl { get; set; }

        public string ImageUrl { get; set; }

        public List<HydrantHeader> NearbyHydrants { get; set; }
    }
}