using System.Collections.Generic;
using Newtonsoft.Json;

namespace gamemaster
{
    public class SlackInteractionPayload
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public SlackInteractionUser User { get; set; }

        [JsonProperty("api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("container")]
        public SlackInteractionContainer Container { get; set; }

        [JsonProperty("trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty("team")]
        public SlackInteractionTeam Team { get; set; }

        [JsonProperty("channel")]
        public SlackInteractionChannel Channel { get; set; }

        [JsonProperty("response_url")]
        public string ResponseUrl { get; set; }

        [JsonProperty("actions")]
        public IList<SlackInteractionAction> Actions { get; set; }
    }
}