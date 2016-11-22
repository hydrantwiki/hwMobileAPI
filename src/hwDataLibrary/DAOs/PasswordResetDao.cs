using HydrantWiki.Library.Objects;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TreeGecko.Library.Mongo.DAOs;

namespace HydrantWiki.Library.DAOs
{
    public class PasswordResetDao : AbstractMongoDAO<PasswordReset>
    {
        public PasswordResetDao(MongoDatabase _mongoDB)
            : base(_mongoDB)
        {
            //Parent - UserGuid
            HasParent = true;
        }

        public override void BuildTable()
        {
            base.BuildTable();

            string[] columns = new string[2] { "ParentGuid", "Code" };

            BuildUniqueIndex(columns, "PARENT_CODE");
        }

        public override string TableName
        {
            get
            {
                return "PasswordReset";
            }
        }

        public void InactivatePasswordResetRequests(Guid _userGuid)
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("ParentGuid", _userGuid.ToString());
            nvc.Add("Active", "True");

            List<PasswordReset> resets = GetList(GetQuery(nvc));

            foreach (var reset in resets)
            {
                reset.Active = false;
                Persist(reset);
            }
        }

        public PasswordReset Get(Guid _userGuid, string _code)
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("ParentGuid", _userGuid.ToString());
            nvc.Add("Code", _code);

            PasswordReset reset = GetOneItem<PasswordReset>(nvc);
            if (reset.Active == false)
            {
                return null;
            }
            return reset;
        }
    }
}
