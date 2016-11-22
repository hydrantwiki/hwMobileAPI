using System;
using System.Security.Cryptography;
using System.Text;
using TreeGecko.Library.Common.Objects;

namespace HydrantWiki.Library.Objects
{
    public class PasswordReset : AbstractTGObject
    {
        public string Code { get; set; } 

        public DateTime CreationDateTime { get; set; }

        public override TGSerializedObject GetTGSerializedObject()
        {
            TGSerializedObject tso = base.GetTGSerializedObject();

            tso.Add("Code", Code);
            tso.Add("CreationDateTime", CreationDateTime);

            return tso;
        }

        public override void LoadFromTGSerializedObject(TGSerializedObject _tg)
        {
            base.LoadFromTGSerializedObject(_tg);

            Code = _tg.GetString("Code");
            CreationDateTime = _tg.GetDateTime("CreationDateTime");
        }

        public static PasswordReset GetNewRequest(Guid _userGuid)
        {
            PasswordReset pr = new PasswordReset
            {
                ParentGuid = _userGuid,
                CreationDateTime = DateTime.UtcNow,
                Code = GetRandomNumericString(8)
            };

            return pr;
        }

        //modified Base64 for regexps 
        private static readonly string[] Characters =
        {
            "1", "2", "3", "4", "5", "6", "7", "8",
            "9", "0"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_length"></param>
        /// <returns></returns>
        private static string GetRandomNumericString(int _length)
        {
            StringBuilder sb = new StringBuilder();

            Byte[] array = GetRandomArray(_length);

            for (int i = 0; i < _length; i++)
            {
                int b = array[i] % 10;

                sb.Append(Characters[b]);
            }

            return sb.ToString();
        }

        private static byte[] GetRandomArray(long _length)
        {
            byte[] arr = new byte[_length];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            rng.GetBytes(arr);

            return arr;
        }
    }
}
