using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackEventApiItem
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("channel")] public string Channel { get; set; }

        [JsonProperty("ts")] public string Ts { get; set; }
    }
}