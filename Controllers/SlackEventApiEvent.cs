using Newtonsoft.Json;

namespace gamemaster.Controllers
{
    public class SlackEventApiEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("authed_teams")]
        public string[] AuthedTeams { get; set; }
        [JsonProperty("event_id")]
        public string EventId { get; set; }
        [JsonProperty("event_time")]
        public int EventTime { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        
        [JsonProperty("reaction")]
        public string Reaction { get; set; }
        
        [JsonProperty("item_user")]
        public string ItemUser { get; set; }
        
        [JsonProperty("event_ts")]
        public string EventTs { get; set; }

        [JsonProperty("item")]
        public SlackEventApiItem Item
        {
            get;
            set;
        }
    }
}