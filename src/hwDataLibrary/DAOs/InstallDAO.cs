using HydrantWiki.Library.Objects;
using MongoDB.Driver;
using TreeGecko.Library.Mongo.DAOs;

namespace HydrantWiki.Library.DAOs
{
    internal class InstallDAO : AbstractMongoDAO<Install>
    {
        public InstallDAO(MongoDatabase _mongoDB)
            : base(_mongoDB)
        {
            HasParent = false;
        }

        public override void BuildTable()
        {
            base.BuildTable();
        }

        public override string TableName
        {
            get
            {
                return "Install";
            }
        }
    }
}
