using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackInteractionChannel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}