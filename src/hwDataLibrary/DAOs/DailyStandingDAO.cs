using HydrantWiki.Library.Objects;
using MongoDB.Driver;
using System;
using TreeGecko.Library.Mongo.DAOs;

namespace HydrantWiki.Library.DAOs
{
    internal class DailyStandingDAO : AbstractMongoDAO<DailyStanding>
    {
        public DailyStandingDAO(MongoDatabase _mongoDB)
            : base(_mongoDB)
        {
            HasParent = false;
        }

        public override string TableName
        {
            get { return "DailyStanding"; }
        }

        public override void BuildTable()
        {
            base.BuildTable();

            BuildUniqueIndex("Date", "STANDING_DATE");
        }

        public DailyStanding GetDailyStanding(DateTime _date)
        {
            return GetOneItem<DailyStanding>("Date", _date.ToString("yyyy-MM-dd"));
        }
    }
}
