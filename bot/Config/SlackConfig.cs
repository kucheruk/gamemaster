using System.Collections.Generic;

namespace gamemaster.Config
{
    public class AppConfig
    {
        public string DefaultCurrency { get; set; } = ":coin:";
        public bool LimitToDefaultCurrency { get; set; }
        public string AnnouncementsChannelId { get; set; }
    }
    
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