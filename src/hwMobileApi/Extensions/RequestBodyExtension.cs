using System.IO;
using Nancy.IO;

namespace HydrantWiki.Mobile.Api.Extensions
{
    public static class RequestBodyExtensions
    {
        public static string ReadAsString(this RequestStream _requestStream)
        {
            string output = null;

            using (var reader = new StreamReader(_requestStream))
            {
                output = reader.ReadToEnd();
            }

            return output;
        }
    }
}