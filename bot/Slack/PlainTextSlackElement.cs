using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class PlainTextSlackElement : SlackElement
    {
        public PlainTextSlackElement(string txt, string tyoe ="plain_text")
        {
            Text = txt;
            Type = tyoe;
        }
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}