using HydrantWiki.Library.Objects;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGecko.Library.Mongo.DAOs;

namespace HydrantWiki.Library.DAOs
{
    internal class ResetFailureDAO : AbstractMongoDAO<ResetFailure>
    {
        public ResetFailureDAO(MongoDatabase _mongoDB)
            : base(_mongoDB)
        {
            //User
            HasParent = true;
        }

        public override string TableName
        {
            get { return "ResetFailure"; }
        }

        public override void BuildTable()
        {
            base.BuildTable();

            List<string> columns = new List<string>() { "ParentGuid", "FailureTicks" };

            BuildNonuniqueIndex(columns, "PARENT_TICKS");
        }

        public int GetCountSince(Guid _userGuid, long ticks)
        {
            var filter =
                Query.And(
                    Query.EQ("ParentGuid", _userGuid.ToString()),
                    Query.GTE("FailureTicks", ticks));
            var cursor = GetCursor(filter);
            int items = (int)cursor.Count();

            return items;
        }
    }
}
