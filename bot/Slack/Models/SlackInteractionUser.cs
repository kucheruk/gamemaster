using Newtonsoft.Json;

namespace gamemaster.Slack.Models
{
    public class SlackInteractionUser
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("team_id")]
        public string TeamId { get; set; }
    }
}