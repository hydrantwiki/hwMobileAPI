namespace HydrantWiki.Mobile.Api.Objects
{
    public class User
    {
        public string AuthorizationToken { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string UserType { get; set; }
    }
}