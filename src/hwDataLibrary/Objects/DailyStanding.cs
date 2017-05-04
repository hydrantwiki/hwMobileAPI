using System;
using TreeGecko.Library.Common.Objects;

namespace HydrantWiki.Library.Objects
{
    public class DailyStanding : AbstractTGObject
    {
        public DailyStanding()
        {
            Date = DateTime.UtcNow;
        }

        public DateTime Date { get; set; }

        public int ActiveUsers { get; set; }

        public override TGSerializedObject GetTGSerializedObject()
        {
            TGSerializedObject tgs = base.GetTGSerializedObject();

            tgs.Add("Date", Date.ToString("yyyy-MM-dd"));
            tgs.Add("ActiveUsers", ActiveUsers);

            return tgs;
        }

        public override void LoadFromTGSerializedObject(TGSerializedObject _tg)
        {
            base.LoadFromTGSerializedObject(_tg);

            ActiveUsers = _tg.GetInt32("ActiveUsers");

            string temp = _tg.GetString("Date");
            if (temp != null)
            {
                DateTime tempDate;
                if (DateTime.TryParse(temp, out tempDate))
                {
                    Date = tempDate;
                }
            }
        }
    }
}
