using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackInteractionAction
    {

        [JsonProperty("action_id")]
        public string ActionId { get; set; }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }

        [JsonProperty("text")]
        public SlackInteractionText Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action_ts")]
        public string ActionTs { get; set; }
    }
}