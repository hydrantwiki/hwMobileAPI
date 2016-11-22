namespace HydrantWiki.Mobile.Api.Objects
{
    public class ResetPassword
    {
        public string Email { get; set; }

        public string Code { get; set; }

        public string InstallId { get; set; }

        public string NewPassword { get; set; }
    }
}