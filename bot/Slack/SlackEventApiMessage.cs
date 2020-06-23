using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackEventApiMessage
    {
        [JsonProperty("token")] public string Token { get; set; }

        [JsonProperty("team_id")] public string TeamId { get; set; }

        [JsonProperty("event_id")] public string EventId { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("challenge")] public string Challenge { get; set; }

        [JsonProperty("event_time")] public int EventTime { get; set; }

        [JsonProperty("api_app_id")] public string ApiAppId { get; set; }

        [JsonProperty("event")] public SlackEventApiEvent Event { get; set; }
    }
}