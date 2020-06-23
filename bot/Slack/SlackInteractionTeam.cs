using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackInteractionTeam
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}