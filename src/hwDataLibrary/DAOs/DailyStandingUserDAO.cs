using HydrantWiki.Library.Objects;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TreeGecko.Library.Mongo.DAOs;

namespace HydrantWiki.Library.DAOs
{
    internal class DailyStandingUserDAO : AbstractMongoDAO<DailyStandingUser>
    {
        public DailyStandingUserDAO(MongoDatabase _mongoDB)
            : base(_mongoDB)
        {
            HasParent = true;
        }

        public override string TableName
        {
            get { return "DailyStandingUser"; }
        }

        public override void BuildTable()
        {
            base.BuildTable();
        }

        public List<DailyStandingUser> GetUserStandings(Guid _dailyStandingGuid)
        {
            return GetChildrenOf(_dailyStandingGuid);
        }

        public void DeleteUserStandings(Guid _dailyStandingGuid)
        {
            DeleteByParent(_dailyStandingGuid);
        }
    }
}
