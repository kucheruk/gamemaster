using System.Collections.Generic;

namespace gamemaster.Config
{
    public class SlackConfig
    {
        public string AppId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SigningSecret { get; set; }
        public string VerificationToken { get; set; }
        public string OauthToken { get; set; }
        public HashSet<string> Admins { get; set; }
    }
}