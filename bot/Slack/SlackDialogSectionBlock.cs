using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackDialogSectionBlock : SlackDialogBlock
    {
        public SlackDialogSectionBlock(string text) : base("section")
        {
            Text = new PlainTextSlackElement(text);
        }
        
        [JsonProperty("text")]
        public PlainTextSlackElement Text { get; set; }
    }
}