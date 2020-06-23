using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class PlainTextSlackElement : SlackElement
    {
        public PlainTextSlackElement(string txt, string type ="plain_text")
        {
            Text = txt;
            Type = type;
        }
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}