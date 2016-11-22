using System.Collections.Generic;
using TreeGecko.Library.Common.Objects;

namespace HydrantWiki.Library.Objects
{
    public class Install : AbstractTGObject
    {
        public Install()
        {
            Usernames = new List<string>();
        }

        public List<string> Usernames { get; set; }

        public override TGSerializedObject GetTGSerializedObject()
        {
            TGSerializedObject tso = base.GetTGSerializedObject();

            tso.Add("Usernames", Usernames);

            return tso;
        }

        public override void LoadFromTGSerializedObject(TGSerializedObject _tg)
        {
            base.LoadFromTGSerializedObject(_tg);

            Usernames = _tg.GetListOfStrings("Usernames");
        }
    }
}
