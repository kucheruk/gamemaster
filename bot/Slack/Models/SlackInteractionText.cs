using Newtonsoft.Json;

namespace gamemaster.Slack.Models
{
    public class SlackInteractionText
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emoji")]
        public bool Emoji { get; set; }
    }
}