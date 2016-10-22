using HydrantWiki.Mobile.Api.Objects;
using System.Collections.Generic;

namespace HydrantWiki.Mobile.Api.ResponseObjects
{
    public class TagsToReviewResponse : BaseResponse
    {
        public List<TagToReview> Tags { get; set; }
    }
}