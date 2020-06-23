using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackOptionElement 
    {
        public SlackOptionElement(string text, string value)
        {
            Value = value;
            Text =new PlainTextSlackElement(text);
        }
        
        [JsonProperty("text")]
        public PlainTextSlackElement Text { get; set; }
        
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}