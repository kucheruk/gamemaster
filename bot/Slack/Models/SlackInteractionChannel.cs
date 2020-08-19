using Newtonsoft.Json;

namespace gamemaster.Slack.Models
{
    public class SlackInteractionChannel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}