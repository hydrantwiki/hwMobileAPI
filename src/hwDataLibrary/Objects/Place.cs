using System.Collections.Generic;

namespace HydrantWiki.Library.Objects
{
    public class Place
    {
        public Place()
        {
            UsersAtThisPosition = new List<UserStats>();
        }

        public int Score { get; set; }

        public List<UserStats> UsersAtThisPosition { get; set; }
    }
}
