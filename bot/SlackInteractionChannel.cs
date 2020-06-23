using Newtonsoft.Json;

namespace gamemaster
{
    public class SlackInteractionChannel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}