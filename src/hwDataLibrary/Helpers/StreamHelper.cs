using System.IO;
using System.Text;

namespace HydrantWiki.Library.Helpers
{
    public static class StreamHelper
    {
        public static Stream AsStream(this string body)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(body));
        }
    }
}
