using System;
using TreeGecko.Library.Common.Objects;

namespace HydrantWiki.Library.Objects
{
    public class DailyStandingUser : AbstractTGObject
    {
        public Guid UserGuid { get; set; }

        public int NewHydrantCount { get; set; }
        
        public int NewHydrantPosition { get; set; }

        public int ApprovedTagCount { get; set; }

        public int ApprovedTagPosition { get; set; }

        public override TGSerializedObject GetTGSerializedObject()
        {
            TGSerializedObject tgs = base.GetTGSerializedObject();

            tgs.Add("UserGuid", UserGuid);
            tgs.Add("NewHydrantCount", NewHydrantCount);
            tgs.Add("NewHydrantPosition", NewHydrantPosition);
            tgs.Add("ApprovedTagCount", ApprovedTagCount);
            tgs.Add("ApprovedTagPosition", ApprovedTagPosition);

            return tgs;
        }

        public override void LoadFromTGSerializedObject(TGSerializedObject _tg)
        {
            base.LoadFromTGSerializedObject(_tg);

            UserGuid = _tg.GetGuid("UserGuid");
            NewHydrantCount = _tg.GetInt32("NewHydrantCount");
            NewHydrantPosition = _tg.GetInt32("NewHydrantPosition");
            ApprovedTagCount = _tg.GetInt32("ApprovedTagCount");
            ApprovedTagPosition = _tg.GetInt32("ApprovedTagPosition");
        }
    }
}

