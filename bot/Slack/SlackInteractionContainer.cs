using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackInteractionContainer
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message_ts")]
        public string MessageTs { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("is_ephemeral")]
        public bool IsEphemeral { get; set; }
    }
}